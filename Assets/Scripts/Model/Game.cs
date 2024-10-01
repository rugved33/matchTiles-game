using System;
using System.Collections;
using Game.Helpers;
using UniRx;

namespace Game.Match3.Model
{
    public class Game: DisposableEntity
    {
        public enum State
        {
            Home,
            Playing,
            Paused,
            Win,
        }

        public BoolReactiveProperty HasStarted { get; }
        public ReactiveCommand ContinueCommand { get; }
        public ReactiveCommand NewMatchCommand { get; }
        public ReactiveCommand RestartCommand { get; }

        public Game()
        {
            HasStarted = new BoolReactiveProperty();

            RestartCommand = new ReactiveCommand();
            RestartCommand.Subscribe(_ => Restart()).AddTo(this);

            NewMatchCommand = new ReactiveCommand();
            NewMatchCommand.Subscribe(_ => NewMatch()).AddTo(this);

            ContinueCommand = new ReactiveCommand(HasStarted);
            ContinueCommand.Subscribe(_ => Continue()).AddTo(this);
        }

        private void Continue()
        {

        }
        private void Restart()
        {

        }
        private void NewMatch()
        {

        }
    }
}