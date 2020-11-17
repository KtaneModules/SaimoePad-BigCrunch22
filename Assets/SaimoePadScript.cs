using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class SaimoePadScript : MonoBehaviour
{
	public KMAudio Audio;
    public KMBombInfo Bomb;
	public KMBombModule Module;
	
	public AudioClip[] SFX;
	public KMSelectable[] Buttons;
	public SpriteRenderer[] FourHead;
	public Sprite[] SpriteLine;
	
	//Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;
	
	int[] Reconcile = Enumerable.Range(0,48).ToArray();
	int[] BooleanVariables = {-1, -1, -1, -1}, ButtonsPressed = {5, 5, 5, 5};
	int StageCount = 0, StageTracker = 0, PressTracker = 0;
	
	void Awake()
    {
		moduleId = moduleIdCounter++;
		for (int a = 0; a < Buttons.Count(); a++)
        {
            int Decide = a;
            Buttons[Decide].OnInteract += delegate
            {
                PressButton(Decide);
				return false;
            };
        }
	}
	
	void Start()
	{
		StageCount = UnityEngine.Random.Range(3,6);
		Debug.LogFormat("[Saimoe Pad #{0}] The module needs {1} correct stages to solve", moduleId, StageCount.ToString());
		Restart();
	}
	
	void PressButton(int Decide)
	{
		Buttons[Decide].AddInteractionPunch(.1f);
		Audio.PlaySoundAtTransform(SFX[1].name, transform);
		if (!ModuleSolved)
		{
			for (int x = 0; x < 4; x++)
			{
				if (Decide == ButtonsPressed[x])
				{
					return;
				}
			}
			
			ButtonsPressed[PressTracker] = Decide;
			BooleanVariables[PressTracker] = Array.IndexOf(SpriteLine, FourHead[Decide].sprite);
			FourHead[Decide].material.color = Color.green;
			PressTracker++;
			
			if (PressTracker == 4)
			{
				string Hem = "You press it in this order: ";
				for (int a = 0; a < 4; a++)
				{
					Hem += a != 3 ? FourHead[ButtonsPressed[a]].sprite.name + ", " : FourHead[ButtonsPressed[a]].sprite.name;
				}
				Debug.LogFormat("[Saimoe Pad #{0}] {1}", moduleId, Hem);
				
				for (int x = 0; x < 3; x++)
				{
					if (BooleanVariables[x] >= BooleanVariables[x + 1])
					{
						Module.HandleStrike();
						Debug.LogFormat("[Saimoe Pad #{0}] The order was not correct. Module striked", moduleId);
						ButtonsPressed = new int[] {5, 5, 5, 5};
						BooleanVariables = new int[] {-1, -1, -1, -1};
						PressTracker = 0;
						Restart();
						foreach(SpriteRenderer a in FourHead)
						{
							a.material.color = Color.white;
						}
						break;
					}
					
					if (x == 2)
					{
						StageTracker++;
						if (StageTracker == StageCount)
						{
							Debug.LogFormat("[Saimoe Pad #{0}] The order was correct. Module solved", moduleId);
							Module.HandlePass();
							ModuleSolved = true;
							Audio.PlaySoundAtTransform(SFX[0].name, transform);
						}
						
						else
						{
							Debug.LogFormat("[Saimoe Pad #{0}] The order was correct. Module advanced a stage", moduleId);
							ButtonsPressed = new int[] {5, 5, 5, 5};
							BooleanVariables = new int[] {-1, -1, -1, -1};
							PressTracker = 0;
							Restart();
							foreach(SpriteRenderer a in FourHead)
							{
								a.material.color = Color.white;
							}
						}
					}
				}
			}
		}
	}
	
	void Restart()
	{
		Reconcile.Shuffle();
		List<int> Varies = new List<int>();
		for (int x = 0; x < 4; x++)
		{
			Varies.Add(Reconcile[x]);
		}
		Varies.Sort();
		
		string Stammer = "Logos displayed: ";
		string Trek = "The correct order to press: ";
		for (int x = 0; x < 4; x++)
		{
			FourHead[x].sprite = SpriteLine[Reconcile[x]];
			Stammer += x != 3 ? FourHead[x].sprite.name + ", " : FourHead[x].sprite.name;
			Trek += x != 3 ? SpriteLine[Varies[x]].name + ", " : SpriteLine[Varies[x]].name;
		}
		Debug.LogFormat("[Saimoe Pad #{0}] {1}", moduleId, Stammer);
		Debug.LogFormat("[Saimoe Pad #{0}] {1}", moduleId, Trek);
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To press the buttons on the Saimoe pad, use the command !{0} press <4 button presses> (Example: !{0} press 1234)";
    #pragma warning restore 414

	string[] ValidNumbers = {"1", "2", "3", "4"};
	
    IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Parameter length invalid. The command was not processed.";
				yield break;
			}
			
			if (parameters[1].Length != 4)
			{
				yield return "sendtochaterror Button press length is not 4. The command was not processed.";
				yield break;
			}
			
			for (int x = 0; x < 4; x++)
			{
				if (!parameters[1][x].ToString().EqualsAny(ValidNumbers))
				{
					yield return "sendtochaterror Command contains an invalid button placement. The command was not processed.";
					yield break;
				}
			}
			
			List<int> Deca = new List<int>();
			foreach (char a in parameters[1])
			{
				Deca.Add(Int32.Parse(a.ToString()));
			}
			Deca.Sort();
			
			for (int x = 0; x < 3; x++)
			{	
				if  (Deca[x] == Deca[x + 1])
				{
					yield return "sendtochaterror All button presses must be different. The command was not processed.";
					yield break;
				}
			}
			
			for (int a = 0; a < 4; a++)
			{
				Buttons[Int32.Parse(parameters[1][a].ToString()) - 1].OnInteract();
				yield return new WaitForSecondsRealtime(0.2f);
			}
		}
	}
}
