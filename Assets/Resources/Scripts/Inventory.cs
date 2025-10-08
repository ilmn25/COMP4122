using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    Dictionary<string, int> _items = new Dictionary<string, int>();

    public Inventory() { }

    public void Add(string id)
    {
        if(_items.ContainsKey(id)) _items[id]++;
        else _items[id] = 1;
    }

    public void Remove(string id)
    {
        if (_items.ContainsKey(id))
        {
            _items[id]--;
            if (_items[id] <= 0) _items.Remove(id);
        }
    }
}