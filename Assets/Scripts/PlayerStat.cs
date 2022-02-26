using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat
{
    private float _health;
    private List<MonsterManager> _cardsOnField;
    private int _id;
    //public PlayerStat Instance;

    public PlayerStat(float health, int id)
    {
        //Instance = this;
        _cardsOnField = null;
        _health = health;
        _id = id;
    }

    public int getId()
    {
        return _id;
    }

    public float GetHealth()
    {
        return _health;
    }

    public void SetHealth(float health)
    {
        _health = health;
    }

    public List<MonsterManager> GetCardsOnField()
    {
        return _cardsOnField;
    }

    public int SetCardsOnField(MonsterManager monster)
    {
        if(_cardsOnField.Count > 3)
        {
            //No more space
            return 0;
        }
        _cardsOnField.Add(monster);
        //Success
        return 1;
    }
}
