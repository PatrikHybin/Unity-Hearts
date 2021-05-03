using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHolder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Player player = GetComponentInParent<Player>();
            player.AddScoreHolder(this.gameObject);
        }
        catch { }
       
    }

}
