using UnityEngine;
using System.Collections;
using DG.Tweening;

public class DeleteAni : MonoBehaviour {
   // public ParticleSystem boom;
    private float shakeTime;
    private float scaleTime;
    private float shakeStrength;
    private Vector3 end;
    private GameObject boom;
    private Tweener tweener2;
    private GameObject boomClone;
    private Renderer thisRenderer;
    void Start () {
        
        shakeTime = 0.6f;
        scaleTime = 0.3f;
        shakeStrength = 0.7f;
        
        end.x = 0;
        end.y = 0;
        end.z = 0;

        boom = Resources.Load("boom")as GameObject;
        if (boom != null)
        {
            boomClone = Instantiate(boom);
        }
        else {
            print("找不到boom啦！！！");
        }   
        
        thisRenderer=this.GetComponent<Renderer>();
        Renderer particleRenderer = boomClone.GetComponent<Renderer>();
        particleRenderer.material=thisRenderer.material ;
        if (particleRenderer.material == null)
        {
            print("找不到Material啦！！！");
        }
        //else {
        //    print(particleRenderer.material.name);
        //}
        
        boomClone.transform.position = this.transform.position;
        ParticleSystem boomParticle = boomClone.GetComponent<ParticleSystem>();
        boomParticle.Play();
        

        Tweener tweener1 = transform.DOShakeScale(shakeTime, shakeStrength);     
        tweener1.OnComplete(Tweener2Ani);

        tweener2 = transform.DOScale(end, scaleTime);
        tweener2.Pause();

       

    }

    void Tweener2Ani() {
        tweener2.Play();
        tweener2.OnComplete(DeleteCube);
    }

     void DeleteCube() {
        Destroy(this.gameObject);
        Destroy(boomClone, 2);

    }
}
