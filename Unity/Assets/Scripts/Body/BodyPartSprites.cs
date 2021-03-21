﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartSprites : MonoBehaviour
{
    private SpriteRenderer _renderer;
    
    [Serializable] public struct MakeShiftDictionaryEntry
    {
        public BodyPartType key;
        public List<Sprite> sprites;
    }

    [SerializeField] private List<MakeShiftDictionaryEntry> spriteDictionary = new List<MakeShiftDictionaryEntry>();
    Dictionary<BodyPartType, List<Sprite>> _sprites = new Dictionary<BodyPartType, List<Sprite>>();
    
    void Awake() 
    {
        _renderer = GetComponent<SpriteRenderer>();
        foreach (var dictEntry in spriteDictionary) _sprites.Add(dictEntry.key, dictEntry.sprites);
    }

    public void SetSprite(BodyPartType bodyPartType, BodyPartState bodyPartState)
    {
        if (!_sprites.ContainsKey(bodyPartType)) return;
        
        _renderer.sprite = _sprites[bodyPartType][bodyPartState == BodyPartState.Outline ? 0 : 1];
    }
}
