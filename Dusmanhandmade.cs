using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Dusmanhandmade : MonoBehaviour
{
    [Header("DİĞER AYARLAR")]
    NavMeshAgent navmesh;
    Animator animatorum;
    GameObject Hedef;
    public GameObject Anahedef;

    [Header("GENEL AYARLAR")]
    public float AtesEtmeMenzilDeger;
    public float SuphelenmeMenzilDeger;
    Vector3 baslangicNoktasi;
    bool Suphelenme = false;
    bool AtesEdiliyormu = false;
    public GameObject AtesEtmeNoktasi;

    [Header("DEVRİYE AYARLARI")]
    public GameObject[] DevriyeNoktalari_1;
    public GameObject[] DevriyeNoktalari_2;
    public GameObject[] DevriyeNoktalari_3;
    GameObject[] AktifOlanNoktaListeleri;

    [Header("SİLAH AYARLAR")]
    float AtesEtmeSikligi_1;
    public float AtesEtmeSikligi_2;
    public float menzil;
    public float DarbeGucu;

    [Header("SESLER")]
    public AudioSource[] Sesler;
    [Header("EFEKTLER")]
    public ParticleSystem[] Efektler;
   

    bool DevriyeVarmi;
    Coroutine DevriyeAt;
    Coroutine DevriyeZaman;
    bool DevriyeKilit;
    public bool DevriyeAtabilirmi;
    float Saglik;


      void Start()
    {
        navmesh = GetComponent<NavMeshAgent>();
        animatorum = GetComponent<Animator>();
        baslangicNoktasi=transform.position;
        StartCoroutine(DevriyeZamanKontrol());
        Saglik = 100;
    }

    GameObject[]  DevriyeKontrol()
    {
                
       int deger = Random.Range(1, 3);
        switch(deger)
        {
                case 1:
                AktifOlanNoktaListeleri = DevriyeNoktalari_1;
                break;
            case 2:
                AktifOlanNoktaListeleri = DevriyeNoktalari_2;
                break;
            case 3:
                AktifOlanNoktaListeleri = DevriyeNoktalari_3;
                break;

        }

        return AktifOlanNoktaListeleri;

       
    }
    IEnumerator DevriyeZamanKontrol()
    {

        while(true && !DevriyeVarmi && DevriyeAtabilirmi)
        {
           
                yield return new WaitForSeconds(5f);

                DevriyeKilit = true;
                StopCoroutine(DevriyeZaman);
                       

        }
    }
    IEnumerator DevriyeTeknikİslem(GameObject[] GelenObjeler)
    {
        navmesh.isStopped = false;
        DevriyeKilit = false;
        DevriyeVarmi = true;
        animatorum.SetBool("yuru", true);
        int toplamnokta = GelenObjeler.Length-1;
        int baslangicdeger = 0;
        navmesh.SetDestination(GelenObjeler[baslangicdeger].transform.position);

        while (true && DevriyeAtabilirmi)
        {

            if (Vector3.Distance(transform.position, GelenObjeler[baslangicdeger].transform.position) <= 1f)
            {

                if (toplamnokta>baslangicdeger)
                {
                    ++baslangicdeger;
                    navmesh.SetDestination(GelenObjeler[baslangicdeger].transform.position);

                }
                else
                {
                    navmesh.stoppingDistance = 1;
                    navmesh.SetDestination(baslangicNoktasi);                  

                }

                
            }
            else
            {

                if (toplamnokta > baslangicdeger)
                {
                    navmesh.SetDestination(GelenObjeler[baslangicdeger].transform.position);

                }
               
            }          
           
            yield return null;

        }


    }
    private void LateUpdate()
    {

        if (navmesh.stoppingDistance==1 && navmesh.remainingDistance <= 1)
        {
            animatorum.SetBool("yuru", false);
            transform.rotation = Quaternion.Euler(0, 180, 0);

            if (DevriyeAtabilirmi)
            {
                DevriyeVarmi = false;
                DevriyeZaman = StartCoroutine(DevriyeZamanKontrol());
                StopCoroutine(DevriyeAt);
            }
                
            navmesh.stoppingDistance = 0;
            navmesh.isStopped = true;

        }


        if (DevriyeKilit && DevriyeAtabilirmi)
        {

            DevriyeAt = StartCoroutine(DevriyeTeknikİslem(DevriyeKontrol()));

           
        }

       SuphelenmeMenzil();
       AtesEtmeMenzil();  
    }

    void AtesEtmeMenzil()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, AtesEtmeMenzilDeger);      

        foreach (var objeler in hitColliders)
        {

            if (objeler.gameObject.CompareTag("Player"))
            {
                AtesEt(objeler.gameObject); 

            }
            else
            {

                if (AtesEdiliyormu)
                {
                    animatorum.SetBool("AtesEt", false);
                    navmesh.isStopped = false;
                    animatorum.SetBool("yuru", true); 
                    AtesEdiliyormu = false;

                }
                
            }

        }

    }

    void AtesEt(GameObject Hedef)
    {
        AtesEdiliyormu = true;
        Vector3 aradakifark = Hedef.gameObject.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(aradakifark, Vector3.up);
        transform.rotation = rotation;
        animatorum.SetBool("yuru", false);
        navmesh.isStopped = true;
        animatorum.SetBool("AtesEt", true);

        RaycastHit hit;        
        if (Physics.Raycast(AtesEtmeNoktasi.transform.position, AtesEtmeNoktasi.transform.forward, out hit, menzil))
        {
            Color color = Color.blue;
            Vector3 degisenpozisyon = new Vector3(Hedef.transform.position.x, Hedef.transform.position.y + 1.5f, Hedef.transform.position.z);
            Debug.DrawLine(AtesEtmeNoktasi.transform.position, degisenpozisyon, color);

            if (Time.time > AtesEtmeSikligi_1)
            {                              
                if(hit.transform.gameObject.CompareTag("Player"))
                {
                    hit.transform.gameObject.GetComponent<KarakterControl>().SaglikDurumu(DarbeGucu);
                    Instantiate(Efektler[1], hit.point, Quaternion.LookRotation(hit.normal));

                }else
                {
                    //Instantiate(Efektler[2], hit.point, Quaternion.LookRotation(hit.normal));

                }
                

                if (!Sesler[0].isPlaying)
                {
                    Sesler[0].Play();
                    Efektler[0].Play();
                }
              
                AtesEtmeSikligi_1 = Time.time + AtesEtmeSikligi_2;


            }
          
          

        }
    }

    void SuphelenmeMenzil()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, SuphelenmeMenzilDeger);        

        foreach (var objeler in hitColliders)
        {


            if (objeler.gameObject.CompareTag("Player"))
            {

                if (animatorum.GetBool("kosma"))
                {
                    animatorum.SetBool("kosma", false);
                    animatorum.SetBool("yuru", true);
                }else
                {
                    animatorum.SetBool("yuru", true);
                }
                
               
                Hedef = objeler.gameObject;
                navmesh.SetDestination(Hedef.transform.position);
                Suphelenme = true;
                if(DevriyeAtabilirmi)
                {
                    StopCoroutine(DevriyeAt);

                }
                
            }
            else
            {

                if (Suphelenme)
                {
                    Hedef = null;               

                    if (transform.position != baslangicNoktasi)
                    {

                        navmesh.stoppingDistance = 1;
                        navmesh.SetDestination(baslangicNoktasi);
                        if (navmesh.remainingDistance <= 1)
                        {
                            animatorum.SetBool("yuru", false);
                            transform.rotation = Quaternion.Euler(0, 180, 0);

                        }


                    }
                    Suphelenme = false;

                    if (DevriyeAtabilirmi)
                    {
                        DevriyeAt = StartCoroutine(DevriyeTeknikİslem(DevriyeKontrol()));

                    }
                   

                }

               



            }

        }

    }

    public void SaglikDurumu(float Darbegucu)
    {
        Saglik -= Darbegucu;

        if(!Suphelenme)
        {
            animatorum.SetBool("kosma", true);          
            navmesh.SetDestination(Anahedef.transform.position);
        }        

        if (Saglik <= 0)
        {           

            animatorum.Play("olme");
            Destroy(gameObject,5f);
        }
         

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AtesEtmeMenzilDeger);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SuphelenmeMenzilDeger);

    }


}
/* RaycastHit hit;

       if(Physics.Raycast(Kafa.transform.position,Kafa.transform.forward, out hit, 10f))
       {
           if (hit.transform.gameObject.CompareTag("Player"))
           {

               Debug.Log("Çarptı");
           }

       }

       Debug.DrawRay(Kafa.transform.position, Kafa.transform.forward * 10f, Color.blue);*/