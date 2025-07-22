using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingMain : MonoBehaviour
{
    public float Time;
    public GameObject Object;



    public void Start()
    {
        StartCoroutine(LoadTime());
    }


    private IEnumerator LoadTime()
    {
        Time = Random.Range(4f, 7f);
        yield return new WaitForSeconds(Time);
        Object.SetActive(false);
    }

}
