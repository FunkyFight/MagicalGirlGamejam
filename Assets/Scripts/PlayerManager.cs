using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    
    [Header("Character Transformations")]
    [SerializeField] private List<GameObject> transformationPrefabs;
    [SerializeField] private int defaultTransformationIndex = 0;
    [SerializeField] private int currentTransformationIndex = -1;
    [SerializeField] private int leftSlotTransformationIndex = -1;
    [SerializeField] private int rightSlotTransformationIndex = -1;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SwitchToCharacter(int characterIndex)
    {

    }
}
