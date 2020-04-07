using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritesTest : MonoBehaviour
{
    [SerializeField] private GameObject spartanPrefab;
    List<GameObject> _spartans;
    // Start is called before the first frame update
    void Start()
    {
        _spartans = new List<GameObject>();

        for(int i=0; i< 300; i++)
        {
            GameObject spartan = Instantiate(spartanPrefab, new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            _spartans.Add(spartan);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //test input
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        direction = Camera.main.transform.TransformDirection(direction);
        direction.y = 0;
        
        
        foreach(GameObject spartan in _spartans)
        {
            //test movement
            spartan.transform.position += direction * 1f * Time.deltaTime;

            //test rotation
            Vector3 horizontalRelation = new Vector3(spartan.transform.position.x, Camera.main.transform.position.y, spartan.transform.position.z);
            Vector3 relativePos = horizontalRelation - Camera.main.transform.position;
            Quaternion relativeRotation = Quaternion.LookRotation(relativePos);
            float angle = Quaternion.Angle(relativeRotation, Camera.main.transform.rotation);
            spartan.transform.rotation = relativeRotation;
        }
    }
}
