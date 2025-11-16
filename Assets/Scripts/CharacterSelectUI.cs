using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnSword;
    public Button btnSpear;
    public Button btnConfirm;

    [Header("Highlights")]
    public GameObject frameSword;   // BtnSword/SelectedFrame
    public GameObject frameSpear;   // BtnSpear/SelectedFrame

    [Header("Optional UI")]
    public TMP_Text descText;

    private CharacterClass? currentPick = null;

    void Start()
    {
        if (frameSword) frameSword.SetActive(false);
        if (frameSpear) frameSpear.SetActive(false);
        if (btnConfirm) btnConfirm.interactable = false;

        if (btnSword) btnSword.onClick.AddListener(() => Select(CharacterClass.Sword));
        if (btnSpear) btnSpear.onClick.AddListener(() => Select(CharacterClass.Spear));
        if (btnConfirm) btnConfirm.onClick.AddListener(Confirm);
    }

    void Select(CharacterClass pick)
    {
        currentPick = pick;

        if (frameSword) frameSword.SetActive(pick == CharacterClass.Sword);
        if (frameSpear) frameSpear.SetActive(pick == CharacterClass.Spear);

        if (descText)
            descText.text = (pick == CharacterClass.Sword)
                ? "Swordsman: Fast movement and strong consecutive slashes are the strengths"
                : "Spear Warrior: Strengths lie in long reach and piercing thrusts";

        if (btnConfirm) btnConfirm.interactable = true; // ← 선택 후 활성화!
    }

    void Confirm()
    {
        if (currentPick == null) return;
        PlayerChoice.Selected = currentPick.Value;
        SceneManager.LoadScene("SampleScene");
    }
}
