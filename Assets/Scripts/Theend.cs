﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Theend : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        var tx = GameObject.FindGameObjectWithTag("Esctoesc").GetComponent<TextMeshProUGUI>().color;
        tx.a = 0f;
        GameObject.FindGameObjectWithTag("Esctoesc").GetComponent<TextMeshProUGUI>().color = tx;
        
        
        
        var tex = GameObject.FindGameObjectWithTag("FinScor").GetComponent<TextMeshProUGUI>();
        tex.text = $"YOU {(Player.Score > 0 ? "WON" : "LOST")}! Your final score was: {Player.Score}";
        StartCoroutine(ShowEscText());
    }
    
    private IEnumerator ShowEscText()
    {
        yield return new WaitForSeconds(3.0f);

        var timer = 0f;

        while ((timer += Time.deltaTime) < 2.0f)
        {
            var tex = GameObject.FindGameObjectWithTag("Esctoesc").GetComponent<TextMeshProUGUI>().color;

            tex.a = Mathf.Lerp(0f, 1f, timer / 2.0f);

            GameObject.FindGameObjectWithTag("Esctoesc").GetComponent<TextMeshProUGUI>().color = tex;
        }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));

        SceneManager.LoadScene("Menu");
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
