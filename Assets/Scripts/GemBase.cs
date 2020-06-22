﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(SpriteRenderer))]
public class GemBase : MonoBehaviour, ITouchHandler {
    
    [HideInInspector]
    public SpriteRenderer spr;

    public GemType type;
    public Vector2Int position;
    public int minMatch = 3;
    
    void Awake() {
        spr = GetComponent<SpriteRenderer>();
    }

    public void SetType(GemData gemData) {
        type = gemData.type;
        spr.sprite = gemData.sprite;
        minMatch = gemData.minMatch;
    }

    public void SetPosition(Vector2Int position) {
        this.position = position;
        BoardController.gemBoard[position.x, position.y] = this;
    }

    public IEnumerator MoveTo(Vector3 target, float duration) {
        
        Vector3 direction = target - transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        float time = 0;

        while(time < duration) {
            transform.position += direction * ((Time.deltaTime * distance)/duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = target;

    }

    public void Matched() {
        StartCoroutine(IEMatched());
    }

    IEnumerator IEMatched() {

        spr.sortingOrder = 1;
        Color c = spr.color;
        c.a = 0.5f;
        spr.color = c;
        transform.localScale = Vector3.one * 1.2f;

        yield return new WaitForSeconds(GameController.instance.swapSpeed);

        StartCoroutine(MoveTo(
            new Vector3(0, -(Camera.main.orthographicSize + 1f)),
            GameController.instance.fallSpeed
        ));
        Destroy(gameObject, GameController.instance.fallSpeed);
    }

    public void TouchDown() {
        
    }

    public void TouchDrag() {
        if(Vector2.Distance(transform.position, TouchController.touchPosition) > 0.75f) {

            Vector2 delta = TouchController.touchPosition - transform.position;
            GemBase otherGem;

            if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {

                int swapX = (int) (position.x + Mathf.Sign(delta.x));

                if(swapX < 0 || swapX >= BoardController.width) {
                    TouchUp();
                    return;
                }

                otherGem = BoardController.gemBoard[swapX, position.y];
            } else {
                
                int swapY = (int) (position.y + Mathf.Sign(delta.y));

                if(swapY < 0 || swapY >= BoardController.height) {
                    TouchUp();
                    return;
                }

                otherGem = BoardController.gemBoard[position.x, swapY];
            }

            if(otherGem) {
                BoardController.TryMatch(this, otherGem);
            }

            TouchUp();
        }
            
    }

    public void TouchUp() {
        TouchController.ClearElementClicked();
    }
}
