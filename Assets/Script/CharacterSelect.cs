using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    private int index;
    [SerializeField] GameObject[] characters;
    [SerializeField] TextMeshProUGUI characterName;

    [SerializeField] GameObject[] characterPrefabs;
    public static GameObject selectedCharacter; 

    void Start()
    {
        index = 0;
        SelectCharacter();
    }
    public void PlayBtnClick()
    {
        SceneManager.LoadScene("clone_Mygarden");
    }

   public void BackBtnClick()
    {
        if (index > 0)
        {
            index--;
        }
        SelectCharacter();
    }
    public void NextBtnClick()
    {
        if (index < characters.Length - 1)
        {
            index++;
        }
        SelectCharacter();
    }
    private void SelectCharacter()
    {
        for(int i=0; i< characters.Length; i++)
        {
            if (i == index)
            {
                characters[i].GetComponent<SpriteRenderer>().color = Color.white;
                characters[i].GetComponent<Animator>().enabled = true;
                selectedCharacter = characterPrefabs[i];
                characterName.text = characterPrefabs[i].name;
            }
            else
            {
                characters[i].GetComponent<SpriteRenderer>().color = Color.black;
                characters[i].GetComponent<Animator>().enabled = false;
            }
        }

        selectedCharacter.tag = "Player";
    }
} 


  

