using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDelete : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource AS;
    public GameObject tempObj;
    private SphereCollider SC;
    private GameObject temp;
    void Start()
    {
        tempObj = Resources.Load("tempObj",typeof(GameObject))as GameObject;
        SC = GetComponent<SphereCollider>();
        temp = Instantiate(tempObj, new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), Quaternion.identity, null);
        temp.transform.localScale = new Vector3(SC.radius * 2, temp.transform.localScale.y, SC.radius * 2);
        AS = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!AS.isPlaying)
        {
            Destroy(temp);
            Destroy(transform.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.transform.CompareTag("Enemy"))
        {
            EnemyBoard EB = other.transform.GetComponent<EnemyBoard>();
            EB.detect = new Vector3(transform.position.x, EB.transform.position.y, transform.position.z);
            EB.detected = true;
        }
    }
}
