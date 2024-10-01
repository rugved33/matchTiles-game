using UnityEngine;
using UniRx;

namespace Game.Match3.Model
{
    public class Character
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public Sprite Icon { get; private set; }
        public CharacterType CharacterType { get; private set; }


        public ReactiveProperty<int> CurrentHealth { get; private set; }
        public float Damage { get; private set; }
        public int MaxHealth { get; private set; }

        public event System.Action<Character> OnDeath;
        public event System.Action<Character> OnRevived;

        public Character(CharacterConfig config)
        {
            Id = config.Id;
            Name = config.Name;
            Icon = config.Icon;
            CharacterType = config.CharacterType;


            MaxHealth = config.CharacterStats.MaxHealth;
            Damage = config.CharacterStats.Damage;
            CurrentHealth.Value = MaxHealth;
            CurrentHealth
                .Where(health => health <= 0)
                .Subscribe(_ => OnDeath?.Invoke(this));

            CurrentHealth
                .Where(health => health <= MaxHealth)
                .Subscribe(_ => OnRevived?.Invoke(this));    
        }

        public void TakeDamage(int amount)
        {
            CurrentHealth.Value -= amount;
            if (CurrentHealth.Value < 0)
            {
                CurrentHealth.Value= 0; 
            }
        }

        public void Heal(int amount)
        {
            CurrentHealth.Value += amount;
            if (CurrentHealth.Value > MaxHealth)
            {
                CurrentHealth.Value = MaxHealth; 
            }
        }

        public void ResetHealth()
        {
            CurrentHealth.Value = MaxHealth;
        }

        public bool IsAlive()
        {
            return CurrentHealth.Value > 0;
        }
    }
}
