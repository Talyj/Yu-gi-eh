using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class MonsterManager : MonoBehaviour
{
    public float life;
    public float damage;
    public float shield;
    public string type;
    public bool hasShield;
    public Animator anim;

    void Update() {
        //Pour les test d'animations, à retirer
        if(Input.GetKeyDown("s")) {
            OnSpawn();
        }
        if(Input.GetKeyDown("m")) {
            Die();
        }
        if(Input.GetKeyDown("p")) {
            GetDamage();
        }
        if(Input.GetKeyDown("a")) {
            Attack();
        }

        //sauf lui

        if(anim.GetCurrentAnimatorStateInfo(0).IsName("death") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
            DestroyEntity();
        }

        if(life <= 0)
        {
            Die();
            DestroyEntity();
        }
    }

    //public pour test a modifier plus tard
    public void OnSpawn() {
        anim.SetBool("Spawn", true);
        Invoke("Reset", 0.2f);
    }

    private void Reset() {
        anim.SetBool("GetDamage", false);
        anim.SetBool("Attack", false);
        anim.SetBool("Spawn", false);
    }

    public void Attack(/*float dmg, MonsterManager target*/) {
        anim.SetBool("Attack", true);
        Invoke("Reset", 0.2f);
        //target.GetDamage();
        //if(target.type == "defender") {
            
        //}
        //if(this.type == "healer") {

        //}
    }
 
    public void GetDamage() {
        anim.SetBool("GetDamage", true);
        Invoke("Reset", 0.2f);
    }

    public void Die() {
        anim.SetBool("Die", true);
    }

    private void DestroyEntity() {
        Destroy(this.gameObject);
    }

}
