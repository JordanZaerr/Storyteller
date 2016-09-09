﻿using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using StoryTeller.Engine.Batching;
using StoryTeller.Engine.UserInterface;
using StoryTeller.Messages;
using StoryTeller.Remotes;
using StoryTeller.Remotes.Messaging;

namespace StoryTeller.Engine
{
    public class EngineAgent : IDisposable
    {
        private readonly IList<IDisposable> _disposables = new List<IDisposable>();
        private SpecificationEngine _engine;
        private object _controller;
        private SocketConnection _socket;
        private Project _project;
        private ISystem _system;
        private SpecExpiration _specExpiration;

        public void Dispose()
        {
            try
            {
                _disposables.ToArray().Each(x => x.SafeDispose());
                _disposables.Clear();
            }
            catch (Exception e)
            {
                ConsoleWriter.Write(ConsoleColor.Red, e.ToString());
            }
        }


        public QueueState QueueState()
        {
            var controller = _controller as EngineController;
            return controller == null ? new QueueState() : controller.QueueState();
        }

        public void Start(EngineMode mode, Project project, int socketPort)
        {
            Project.CurrentProject = project;

            _socket = new SocketConnection(socketPort, false, (s, json) =>
            {
                EventAggregator.Messaging.SendJson(json);
            });

            _disposables.Add(_socket);

            EventAggregator.Start(_socket);

            _project = project;

            Type systemType = null;

            try
            {
                systemType = _project.DetermineSystemType();
                _system = Activator.CreateInstance(systemType).As<ISystem>();
                _disposables.Add(_system);

                _specExpiration = new SpecExpiration();

                _engine = mode == EngineMode.Interactive
                    ? buildUserInterfaceEngine()
                    : buildBatchedEngine(project.TracingStyle);


                _engine.Start(project.StopConditions);


            }
            catch (Exception e)
            {
                var message = new SystemRecycled
                {
                    error = e.ToString(),
                    success = false,
                };

                if (systemType != null)
                {
                    message.system_name = systemType.AssemblyQualifiedName;
                }

                EventAggregator.SendMessage(message);
            }


        }

        private SpecificationEngine buildUserInterfaceEngine()
        {
            var observer = new UserInterfaceObserver();

            var runner = new SpecRunner(new UserInterfaceExecutionMode(observer), _system, _specExpiration);

            var executionObserver = new UserInterfaceExecutionObserver();
            var engine = new SpecificationEngine(_system, runner, executionObserver);
            _controller = new EngineController(engine, observer, runner);

            // Super hokey, but we need some way to feed the spec started
            // event up to EngineController
            // TODO -- maybe pull the IExecutionQueue concept from 1-2 back out
            executionObserver.Controller = (EngineController)_controller;


            _disposables.Add(observer);
            _disposables.Add(engine);

            EventAggregator.Messaging.AddListener(_controller);

            return engine;
        }

        private SpecificationEngine buildBatchedEngine(string tracingStyle)
        {
            IBatchObserver batchObserver = new BatchObserver();
            IExecutionObserver executionObserver = new NulloObserver();

            if ("TeamCity" == tracingStyle)
            {
                batchObserver = new TeamCityBatchObserver(batchObserver);
                executionObserver = new TeamCityExecutionObserver();
            }

            var executionMode = new BatchExecutionMode(batchObserver);
            var runner = new SpecRunner(executionMode, _system, _specExpiration);

            var engine = new SpecificationEngine(
                _system, runner, executionObserver);

            _controller = new BatchController(engine, batchObserver);

            _disposables.Add(engine);

            EventAggregator.Messaging.AddListener(_controller);

            return engine;
        }
    }
}