﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private const int SLOTS = 4;

    private List<IInventoryItem> mItems = new List<IInventoryItem>();

    public event EventHandler<InventoryEventArgs> ItemAdded;

    public void AddItem(IInventoryItem item)
    {
        if(mItems.Count < SLOTS)
        {
            Collider collider = (item as MonoBehaviour).GetComponent<Collider>();
            if(collider.enabled)
            {
                collider.enabled = false;

                mItems.Add(item);

                item.OnPickup();
            }

            if(ItemAdded != null)
            {
                //Use this to notify the HUD to update
                ItemAdded(this, new InventoryEventArgs(item));
            }
        }
    }

    //public byte[] InventoryBytes()
    //{
    //    byte[] inventory;
    //    for(IInventoryItem item : mItems)
    //    {

    //    }
    //}
}