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
    public GameObject frameSword;
    public GameObject frameSpear;

    [Header("Optional UI")]
    public TMP_Text descText;

    CharacterClass? currentPick = null;

    void Awake()
    {
        AutoCache();
    }

    void Start()
    {
        ResetUI();

        if (btnSword) btnSword.onClick.AddListener(() => Select(CharacterClass.Sword));
        if (btnSpear) btnSpear.onClick.AddListener(() => Select(CharacterClass.Spear));
        if (btnConfirm) btnConfirm.onClick.AddListener(Confirm);
    }

    void AutoCache()
    {
        if (btnSword == null)
            btnSword = FindButton("BtnSword");
        if (btnSpear == null)
            btnSpear = FindButton("BtnSpear");
        if (btnConfirm == null)
            btnConfirm = FindButton("BtnConfirm");

        if (frameSword == null && btnSword != null)
        {
            var tr = btnSword.transform.Find("SelectedFrame");
            frameSword = tr ? tr.gameObject : null;
        }
        if (frameSpear == null && btnSpear != null)
        {
            var tr = btnSpear.transform.Find("SelectedFrame");
            frameSpear = tr ? tr.gameObject : null;
        }

        if (descText == null)
            descText = GetComponentInChildren<TMP_Text>();
    }

    Button FindButton(string name)
    {
        var tr = transform.Find(name);
        return tr ? tr.GetComponent<Button>() : null;
    }

    void ResetUI()
    {
        if (frameSword) frameSword.SetActive(false);
        if (frameSpear) frameSpear.SetActive(false);
        if (btnConfirm) btnConfirm.interactable = false;
        if (descText) descText.text = "";
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

        if (btnConfirm) btnConfirm.interactable = true;
    }

    void Confirm()
    {
        if (currentPick == null) return;
        PlayerChoice.Selected = currentPick.Value;
        SceneManager.LoadScene("SampleScene");
    }
}

