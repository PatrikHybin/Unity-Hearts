using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameHolder : MonoBehaviour
{ 
    void Awake()
    {
        Player player = GetComponentInParent<Player>();
        player.AddNameHolder(this.gameObject);    
    }
}
