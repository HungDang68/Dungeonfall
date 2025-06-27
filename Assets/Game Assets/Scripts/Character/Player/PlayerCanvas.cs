using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCanvas : MonoBehaviour
{
    public Slider healthSlider;
    public Slider EXPSlider;
    public TextMeshProUGUI levelText;

    public void SetMaxHealth(int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }
    public void SetHealth(int health)
    {
        healthSlider.value = health;
    }
    public void SetEXP(int currentEXP, int nextLevelsEXP)
    {
        EXPSlider.value = currentEXP;
        EXPSlider.maxValue = nextLevelsEXP;
    }
    public void SetLevel(int level)
    {
        levelText.text = level.ToString();
    }
}
