using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatEnemiesQuestStep : QuestStep
{
    
    private int enemiesDefeated = 0;
    //Num of enemies we want to defeat in the tutorial
    private int enemyGoal = 10;

    private void OnEnable()
    {
        //When we have a listener for enemy defeats put it here
    }

    private void OnDisable()
    {
        //When we have a listener for enemy defeats put it here
    }

    private void EnemiesDefeated()
    {
        if (enemiesDefeated < enemyGoal)
        {
            enemiesDefeated++;
        }

        if (enemiesDefeated >= enemyGoal)
        {
            FinishQuestStep();
        }
    }
}
