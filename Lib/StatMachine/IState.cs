/*
 MADE 7rzr 2024-01-06
 update 2023-12-14 로딩창 의존성 제거


*/

  
  


using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace KD
{
    public interface IState<T>
    {
        public public IState<T> StateMachine { get; set; }
        public int StateId { get; set; }

        void OnEnter(IState<T> _prev);

        void OnUpdate(float _deltaTime);

        void OnExit(IState<T> _next);
    }
}






