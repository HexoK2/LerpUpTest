using UnityEngine;

// Déplace le personnage avec les flèches (Input Manager classique).
public class DeplacementJoueur : MonoBehaviour
{
    [SerializeField] float vitesse = 3f;   // en unités par seconde

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 direction = new Vector2(x, y).normalized;
        transform.Translate(direction * vitesse * Time.deltaTime);
    }
}
