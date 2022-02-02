using UnityEngine;
public class GameCharacter : MonoBehaviour
{
    public int Health { get => health; set => health = value; }
    public bool IsAlive { get => isAlive; }
    [SerializeField] public int maxHealth;
    public int health;
    public int damage;
    private bool isAlive = true;
    private void Awake()
    {
        ResetStats();
    }
    public virtual void ResetStats()
    {
        Heal(maxHealth);
        isAlive = true;
    }
    public virtual void TakeDamage(int damage, Transform character)
    {
        //DamageFeedback(character);
        health -= damage;
        //if(health <= 0)
        //{
            //Death();
        //}
    }
    public void Heal(int heal)
    {
        health += heal;
        if (health > maxHealth) health = maxHealth;
    }
    protected virtual void Death()
    {
        health = 0;
        isAlive = false;
        //gameObject.SetActive(false);
    }
    protected virtual void DamageFeedback(Transform character)
    {
        character.transform.Translate(Vector3.back);
    }

}
