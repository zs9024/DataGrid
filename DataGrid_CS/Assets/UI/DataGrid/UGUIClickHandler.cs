
// using MogoEngine.Timer;
// using MogoEngine.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MogoEngine.UISystem
{
    public class UGUIClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public delegate void PointerEvetCallBackFunc(GameObject target, PointerEventData eventData);
        //public string m_sound = AB.AUDIO_UI;
        public event PointerEvetCallBackFunc onPointerClick;

        private bool m_isCanClick = true;
        private uint m_iCDTime = 1000;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Input.touchCount > 1)//Edit by limanru
                return;

            if (!m_isCanClick)
            {
                return;
            }

            //if (!string.IsNullOrEmpty(m_sound))
            //    AudioManager.PlayUISound(m_sound);

            if (onPointerClick != null)
                onPointerClick(gameObject, eventData);
            else
                Debug.LogError("系统暂未开放");

            //add cd-time. Edit by limanru
            if (m_iCDTime > 0)
            {
                m_isCanClick = false;
                //MogoTimer.AddTimer(m_iCDTime, 0, () => m_isCanClick = true);
            }
        }

        public void RemoveAllHandler(bool isDestroy = true)
        {
            onPointerClick = null;
            if (isDestroy) DestroyImmediate(this);
        }

        public UGUIClickHandler SetCDTime(float seconds)
        {
            m_iCDTime = (uint)(seconds * 1000);
            return this;
        }

        public static UGUIClickHandler Get(GameObject go)
        {
            UGUIClickHandler listener = go.GetComponent<UGUIClickHandler>();
            if (listener == null)
                listener = go.AddComponent<UGUIClickHandler>();
            return listener;
        }

        public static UGUIClickHandler Get(GameObject go, string sound)
        {
            UGUIClickHandler listener = Get(go);
            //listener.m_sound = sound;
            return listener;
        }

        public static UGUIClickHandler Get(Transform tran, string sound)
        {
            UGUIClickHandler listener = Get(tran);
            //listener.m_sound = sound;
            return listener;
        }

        public static UGUIClickHandler Get(Transform tran)
        {
            return Get(tran.gameObject);
        }
    }

}
