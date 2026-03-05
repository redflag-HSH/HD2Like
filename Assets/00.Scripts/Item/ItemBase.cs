using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemBase : Interactor
{
    private void Awake()
    {
        float[] aa = new float[3];
        //랜덤하게 회전하는 함수
        for (int j = 0; j < 3; j++)
            aa[j] = Random.Range(-30.0f, 31.0f);
        transform.Rotate(aa[0], aa[1], aa[2]);
        StartCoroutine(fallOff());
    }
    public override void OnInteract(PlayingMovement m)
    {
        this.gameObject.SetActive(false);
    }
    IEnumerator fallOff()
    {
        GetComponent<Rigidbody>().useGravity = true;
        yield return new WaitForSeconds(1.0f);
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }
}
