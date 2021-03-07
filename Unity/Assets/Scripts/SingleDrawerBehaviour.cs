﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SingleDrawerBehaviour : MonoBehaviour
{
    public SingleDrawerBehaviour pair;

    private Camera _camera;
    private Transform _transform;
    private Vector3 _transformPosition;
    
    [Header("Drawer stuff")]
    [SerializeField] float moveRange = 2.4f;
    bool _placedHorizontally;
    bool _placedOnTheLeft;
    bool _placedOnTheBottom;
    
    [SerializeField] float _movementPercentage = 0;
    Vector3 _startingPosition;
    Vector3 _finalPosition;
    Vector3 _targetPosition;

    [Header("While Being Grabbed parameters")]
    [SerializeField] float shrinkPercent = 1.05f;
    Vector2 _grabPos;
    Collider2D _collider;
    bool _isGrabbed;


    private void Awake() {
        _camera = Camera.main;
        _collider = GetComponent<Collider2D>();
        _transform = transform;
        
        _placedHorizontally = _transform.rotation.z == 0;
        _placedOnTheLeft = _transform.position.x < 0;
        _placedOnTheBottom = _transform.position.y < 0;

        _startingPosition = _transform.position;
        _finalPosition = _startingPosition + 
        new Vector3(
                    (_placedHorizontally?moveRange:0)*(_placedOnTheLeft?1:-1), 
                    (_placedHorizontally?0:moveRange)*(_placedOnTheBottom?1:-1)
                    );
    }
    
    void Update()
    {
        if (Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = _camera.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Began){
                if (_collider == Physics2D.OverlapPoint(touchPos)){
                    _isGrabbed = true;
                    _transform.DOScale(shrinkPercent, 0.1f);
                    _grabPos = touchPos;
                }
            }

            else if (touch.phase == TouchPhase.Moved && _isGrabbed){
                Vector2 difference = touchPos - _grabPos;
                _targetPosition = _placedHorizontally 
                ? new Vector3(_transform.position.x + difference.x, _transform.position.y) 
                : new Vector3(_transform.position.x, _transform.position.y + difference.y);

                _targetPosition = Utils.NormalizedWithBounds(_targetPosition, _startingPosition, _finalPosition);
                _movementPercentage = Utils.Vector3InverseLerp(_startingPosition, _finalPosition, _targetPosition);
                
                // So drawers don't stay stuck in awkward mid positions
                if (_movementPercentage < 0.4f)
                    DOTween.To(() => _movementPercentage, x => _movementPercentage = x, 0, 0.1f);
                else if (_movementPercentage > 0.6f) 
                    DOTween.To(() => _movementPercentage, x => _movementPercentage = x, 1, 0.1f);
            }

            else if (touch.phase == TouchPhase.Ended){
                _isGrabbed = false;
                _transform.DOScale(1, 0.1f);
            }
        }
        
        // Make sure pairs move together 
        if (pair) pair._movementPercentage = _movementPercentage;
        _transform.position = Vector3.Lerp(_startingPosition, _finalPosition, _movementPercentage);

    }
}