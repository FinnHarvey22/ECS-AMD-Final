using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class Enemy : ScriptableObject
{
	public string name;
	public int ID;
	public float health;
	public float speed;
	public float damage;

}
