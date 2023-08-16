using UnityEngine;
using TMPro;

public class SetNickName : MonoBehaviour
{
    public TextMeshProUGUI inputField;

    void Update()
    {
        //InputFieldに何も入れないと"ゼロ幅スペース"なるものが代入されている
        //ゼロ幅スペースは \u200b (char型だと8203)
        string playerName = inputField.text.Replace("\u200b", "");

        if (System.String.IsNullOrWhiteSpace(playerName)){
            playerName = "Player";
        }
        else{
            playerName = inputField.text.Replace(" ", "_");
            playerName =playerName.Replace("　", "_");
        }

        NetworkManager.Instance.nickName = playerName;
    }
}