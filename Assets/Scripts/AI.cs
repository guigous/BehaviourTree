using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{

    //Declaracao de variaveis
    public Transform player;
    public Transform bulletSpawn;
    public Slider healthBar;
    public GameObject bulletPrefab;
    public LayerMask obstacleLayer;


    NavMeshAgent agent;
    public Vector3 destination; // The movement destination.
    public Vector3 target;      // The position to aim to.
    float health = 100.0f;
    float rotSpeed = 30.0f;

    float visibleRange = 80.0f;
    float shotRange = 40.0f;
    float nextFireTime = 1f;
    float fireRate = 0.1f;

    void Start()
    {
        //pegando componentes, setando alguns parametros para o agente do navmesh
        //e iniciando o metodo de recuperar vida
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        InvokeRepeating("UpdateHealth", 5, 0.5f);
    }

    void Update()
    {
        //Atualizando a posicao da barra de vida

        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)health;
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);
    }


    //metodo chamado no start
    void UpdateHealth()
    {
        

        if (health < 100)
            health++;
    }

    //deteccao de colisao da bala e execucao da logica
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }


    //adquirindo um vector3 aleatorio
    //comunicando ao agent do navmesh
    //retornando a arvore de comportamento o resultado
    [Task]
    void PickRandomDestination()
    {
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    //Movendo o personagem para o ponto predeterminado
    [Task]
    void MoveToDestination()
    {
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    //Checando se a vida do inimigo e menor que a variavel
    //executando a destruicao do gameobject
    [Task]
    public bool IsHealthLessThan(float health)
    {
        return this.health < health;
    }
    [Task]
    public bool Explode()
    {
        Destroy(healthBar.gameObject);
        Destroy(this.gameObject);
        return true;
    }


    //metodos para olhar e mirar no player
    [Task]
    void TargetPlayer()
    {
        Vector3 playerDirection = player.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        
    }

    [Task]
    void LookAtTarget()
    {
        Vector3 playerDirection = player.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(playerDirection);
        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        
    }


    //Lógica de disparo do tanque inimigo
    [Task]
    void Fire()
    {
        if (Time.time >= nextFireTime)
        {
            // Lógica de disparo do tanque inimigo
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.rotation);
            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            bulletRigidbody.AddForce(bulletSpawn.transform.forward * 200, ForceMode.Impulse);

            nextFireTime = Time.time + fireRate;
        }

    }


    //deteccao do player dentro do alcance
    [Task]
    bool SeePlayer()
    {
        Vector3 playerDirection = player.position - transform.position;
        float playerDistance = playerDirection.magnitude;

        // Verifica a distância do jogador
        if (playerDistance > visibleRange)
            return false;

        // Verifica a linha de visão
        if (!Physics.Raycast(transform.position, playerDirection, playerDistance, obstacleLayer))
            return true;

        return false;
    }
}

