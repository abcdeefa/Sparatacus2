using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController2D player;  // 플레이어 스크립트
    public Image hpFill;               // HpBarFill 이미지

    void Update()
    {
        if (player == null || hpFill == null) return;

        float ratio = (float)player.CurrentHp / player.maxHp;
        ratio = Mathf.Clamp01(ratio);

        hpFill.fillAmount = ratio;
    }
}
