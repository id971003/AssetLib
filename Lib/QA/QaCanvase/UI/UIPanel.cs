
using System;
using UnityEngine;

namespace dk.UI
{
    public class UIPanel : MonoBehaviour
    {
        public Action OnShow;
        public Action OnHide;

        [SerializeField] protected bool useAnimation = true;

        
        [SerializeField] protected Animator anim;

        protected virtual void Awake()
        {
            if (useAnimation)
            {
                if (anim == null)
                {
                    anim = GetComponent<Animator>();
                }
            }
        }

        public virtual void Toggle()
        {
            if(gameObject.activeInHierarchy)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Toggle(bool show)
        {
            if (show)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            if(useAnimation) 
                anim.SetTrigger("Show");
            OnShow?.Invoke();
        }

        public virtual void Hide()
        {
            if (useAnimation)
            {
                anim.SetTrigger("Hide");
            }
            else
            {
                gameObject.SetActive(false);
            }
            OnHide?.Invoke();
        }

        public virtual void ShowEvent()
        {
            gameObject.SetActive(true);
        }

        public virtual void HideEvent()
        {
            gameObject.SetActive(false);
        }
    }
}