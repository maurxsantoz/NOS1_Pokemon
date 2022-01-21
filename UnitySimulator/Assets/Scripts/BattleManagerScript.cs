using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleManagerScript : MonoBehaviour
{
    [SerializeField] GameObject menuControlleur;
    Pokemon playerPokemon;
    [SerializeField] GameObject playerName;
    [SerializeField] GameObject playerSprite;
    [SerializeField] GameObject playerHpSlider;
    [SerializeField] GameObject playerHpLabel;
    Pokemon enemyPokemon;
    [SerializeField] GameObject enemyName;
    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject enemyHpSlider;
    [SerializeField] List<GameObject> moveLabels;
    public static BattleManagerScript _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            //playerPokemon = Database.Instance.FindPokemonBy(59);
            //StartGame(playerPokemon);
        }
    }

    public void StartGame(Pokemon playerPokemon)
    {
        this.playerPokemon = playerPokemon;
        GenerateEnemyPokemon();
        LoadUi();
    }

    public void UpdateUi()
    {
        UpdateHp(true);
        UpdateHp(false);
        UpdateMoves();
    }

    private void LoadUi()
    {
        LoadSprites();
        playerName.GetComponent<TextMeshProUGUI>().SetText(playerPokemon.Name);
        enemyName.GetComponent<TextMeshProUGUI>().SetText(enemyPokemon.Name);
        UpdateHp(true);
        UpdateHp(false);
        UpdateMoves();
    }

    private void UpdateMoves()
    {
        for (int i = 0; i < playerPokemon.SelectedMoves.Count; i++)
        {
            moveLabels[i].GetComponent<TextMeshProUGUI>().text = playerPokemon.SelectedMoves[i].Name;
            string message = "PP: " + playerPokemon.SelectedMoves[i].Pp + "/" + playerPokemon.SelectedMoves[i].MaxPp +
                             "\n Type: " + playerPokemon.SelectedMoves[i].Type.Name + "\n Class: " +
                             playerPokemon.SelectedMoves[i].DamageClass;
            moveLabels[i].transform.parent.GetChild(1).gameObject.GetComponent<UseTooltipScript>().Message = message;
        }
    }

    private void UpdateHp(bool player)
    {
        Pokemon pokemon;
        GameObject slider;
        if (player)
        {
            playerHpLabel.GetComponent<TextMeshProUGUI>()
                .SetText(playerPokemon.BaseStats.Hp + " / " + playerPokemon.BaseStats.MaxHp);
            pokemon = playerPokemon;
            slider = playerHpSlider;
        }
        else
        {
            pokemon = enemyPokemon;
            slider = enemyHpSlider;
        }

        slider.GetComponent<Image>().fillAmount = (1f * pokemon.BaseStats.Hp / pokemon.BaseStats.MaxHp);
    }

    private void GenerateEnemyPokemon()
    {
        enemyPokemon = Database.Instance.FindPokemonBy(25);
        Move move = Database.Instance.FindMoveBy("tackle");
        enemyPokemon.SelectedMoves.AddRange(Enumerable.Repeat(move, 4).ToList());
    }

    private void LoadSprites()
    {
        enemySprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pokemon/Front/" + enemyPokemon.Dex);
        playerSprite.GetComponent<SpriteRenderer>().sprite =
            Resources.Load<Sprite>("Pokemon/Back/" + playerPokemon.Dex);
    }

    private void LoadMoves()
    {
        Sprite loadedSprite = Resources.Load<Sprite>("Pokemon/Front/" + enemyPokemon.Dex);
        enemySprite.GetComponent<SpriteRenderer>().sprite = loadedSprite;
        playerSprite.GetComponent<SpriteRenderer>().sprite =
            Resources.Load<Sprite>("Pokemon/Back/" + playerPokemon.Dex);
    }

    public void MoveClicked(int index)
    {
        if (playerPokemon.SelectedMoves[index].Pp > 0)
        {
            StartCoroutine(AttackSequence(index));
        }
        else
        {
            //play error Sound
        }
    }

    IEnumerator AttackSequence(int playerMove)
    {
        ToggleButtons();
        Pokemon fasterPokemon, slowerPokemon;
        int fastPokemonMove, slowPokemonMove;
        if (playerPokemon.BaseStats.Speed > enemyPokemon.BaseStats.Speed)
        {
            fasterPokemon = playerPokemon;
            fastPokemonMove = playerMove;
            slowerPokemon = enemyPokemon;
            slowPokemonMove = Random.Range(0, 3);
        }
        else if (playerPokemon.BaseStats.Speed < enemyPokemon.BaseStats.Speed)
        {
            fasterPokemon = enemyPokemon;
            fastPokemonMove = Random.Range(0, 3);
            slowerPokemon = playerPokemon;
            slowPokemonMove = playerMove;
        }
        else if (Random.value >= 0.5)
        {
            fasterPokemon = playerPokemon;
            fastPokemonMove = playerMove;
            slowerPokemon = enemyPokemon;
            slowPokemonMove = Random.Range(0, 3);
        }
        else
        {
            fasterPokemon = enemyPokemon;
            fastPokemonMove = Random.Range(0, 3);
            slowerPokemon = playerPokemon;
            slowPokemonMove = playerMove;
        }

        fasterPokemon.Attack(fasterPokemon.SelectedMoves[fastPokemonMove], slowerPokemon);
        yield return new WaitForSeconds(1);
        if (slowerPokemon.BaseStats.Hp > 0)
        {
            slowerPokemon.Attack(slowerPokemon.SelectedMoves[slowPokemonMove], fasterPokemon);
            yield return new WaitForSeconds(1);
        }

        CheckWinner();
        ToggleButtons();
    }

    public void ToggleButtons()
    {
        for (int i = 0; i < 4; i++)
        {
            moveLabels[i].transform.parent.gameObject.SetActive(!moveLabels[i].transform.parent.gameObject.activeSelf);
        }

        TooltipScript._instance.HideTooltip();
    }

    public void CheckWinner()
    {
        if (enemyPokemon.BaseStats.Hp == 0)
        {
            //player won

            menuControlleur.GetComponent<MenuControllerScript>().EndBattle(playerPokemon, "Victory!");
        }
        else if (playerPokemon.BaseStats.Hp == 0)
        {
            //player lost lol
            menuControlleur.GetComponent<MenuControllerScript>().EndBattle(enemyPokemon, "Defeat!");
        }
    }
}