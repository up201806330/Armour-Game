﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Body.BodyType;
using Newtonsoft.Json;
using UI;
using UnityEngine;
using Utils;

namespace Body
{
    public class FinishedBodiesController : MonoBehaviour
    {
        private List<FinishedBody> _controllers;
        private readonly Dictionary<BodyPartType, Dictionary<BodyPartState, BodyPartBehaviour>> _bodyParts = new Dictionary<BodyPartType, Dictionary<BodyPartState, BodyPartBehaviour>>();

        private SplitBodiesButton _splitBodiesButton;
        private bool _oldIsOverlapping;
        private EmotionBoxes _emotionBoxes;

        public int saveSlot;
        private readonly Dictionary<BodyPartType, BodyInputInfo> _bodyInputInfo = new Dictionary<BodyPartType, BodyInputInfo>();
        
        [SerializeField] private ReturnParameters returnParameters;
        
        void Awake()
        {
            _controllers = GetComponentsInChildren<FinishedBody>().ToList();
            _splitBodiesButton = SplitBodiesButton.Instance;
            _oldIsOverlapping = _splitBodiesButton.isOverlapping;
            _emotionBoxes = GetComponentInChildren<EmotionBoxes>();
            _emotionBoxes.BodyInputInfo = _bodyInputInfo;
        }

        private void Start()
        {
            // Ran after Awake to make sure body parts have been loaded
            var bodyPartBehaviours = GameObject.FindGameObjectsWithTag("Bodypart")
                .Select(obj => obj.GetComponentInParent<BodyPartBehaviour>());
            foreach (var bodyPartBehaviour in bodyPartBehaviours)
            {
                if (!_bodyParts.ContainsKey(bodyPartBehaviour.BodyType))
                    _bodyParts[bodyPartBehaviour.BodyType] = new Dictionary<BodyPartState, BodyPartBehaviour>()
                    {
                        {bodyPartBehaviour.BodyPartState, bodyPartBehaviour}
                    };
                else 
                    _bodyParts[bodyPartBehaviour.BodyType][bodyPartBehaviour.BodyPartState] = bodyPartBehaviour;
            }
        }

        private void Update()
        {
            if (_splitBodiesButton.isOverlapping == _oldIsOverlapping) return;

            _oldIsOverlapping = _splitBodiesButton.isOverlapping;
            _emotionBoxes.ToggleVisible();
        }

        public bool IsFinished => _bodyInputInfo.Count == 6;
        
        public void InsertBodyInputInfo(BodyPartType bodyPartType, BodyInputInfo bodyInputInfo, bool skipSave)
        {
            _bodyInputInfo[bodyPartType] = bodyInputInfo;

            foreach (var controller in _controllers)
            {
                controller.InsertBodyInputInfo(bodyPartType, bodyInputInfo);
            }

            if (!skipSave)
                SaveGame();
        }

        [ContextMenu("Save Game")]
        public void SaveGame()
        {
            // Make sure saves folder exists
            string saveFolder = Application.persistentDataPath + "/saves";
            Directory.CreateDirectory(saveFolder);

            var serialized = JsonConvert.SerializeObject(_bodyInputInfo);
            string destination = saveFolder + "/save_" + saveSlot + ".dat";
            File.WriteAllText(destination, serialized);
            print("Saved " + serialized);
        }

        [ContextMenu("Load Game")]
        public void LoadGame(Dictionary<BodyPartType, BodyInputInfo> state, Action doAfterHandler)
        {
            _bodyInputInfo.Clear();
            StartCoroutine(PlaceParts_CO(state, doAfterHandler));
        }

        private IEnumerator PlaceParts_CO(Dictionary<BodyPartType, BodyInputInfo> bodyInputInfos, Action doAfterHandler)
        {
            foreach (KeyValuePair<BodyPartType, BodyInputInfo> pair in bodyInputInfos)
            {
                _bodyParts[pair.Key][BodyPartState.Physical].PlaceCorrectly(pair.Value);
                yield return new WaitForSeconds(returnParameters.returnDuration / 2);
                _bodyParts[pair.Key][BodyPartState.Disease].PlaceCorrectly(pair.Value);
                yield return new WaitForSeconds(returnParameters.returnDuration / 2);
            }
            
            doAfterHandler();
        }
    }
}
