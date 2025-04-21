using UnityEngine;
using UnityEngine.UI;

namespace Rpg
{
    public class UnitUI : MonoBehaviour
    {
        public void BindUnit(Unit unit)
        {
            unit.OnHpChange += UpdateHpUI;
        }

        public void UnbindUnit(Unit unit)
        {
            unit.OnHpChange -= UpdateHpUI;
        }

        public Image hpBar;
        public void UpdateHpUI(int curHp, int maxHp)
        {
            hpBar.fillAmount = curHp / (float)maxHp;
        }
    }    
}
