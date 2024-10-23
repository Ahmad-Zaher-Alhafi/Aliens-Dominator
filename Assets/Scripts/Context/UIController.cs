using UI;
using UnityEngine;

namespace Context {
    public class UIController : MonoBehaviour {
        [SerializeField] private SuppliesUI suppliesUI;
        public SuppliesUI SuppliesUI => suppliesUI;
    }
}