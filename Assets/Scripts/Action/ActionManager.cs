using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class ActionManager
{
    Manager manager;
    List<Action> actions = new List<Action>();

    public ActionManager(Manager manager)
    {
        this.manager = manager;
    }

    public void addAction(Action action)
    {
        actions.Add(action);
    }

    public void update()
    {
        foreach(Action action in actions)
        {
            if (action.canExecute(manager))
                action.execute(manager);
        }
    }
}
