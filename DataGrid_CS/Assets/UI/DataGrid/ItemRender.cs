using UnityEngine;
using System.Collections;
namespace MogoEngine.UISystem
{
    public abstract class ItemRender : MonoBehaviour
    {
        public object m_renderData;
        [HideInInspector]
        public DataGrid m_owner;
        public abstract void Awake();
        protected abstract void OnSetData(object data);

        public void SetData(object data)
        {
            m_renderData = data;
            OnSetData(data);
        }
    }
}
