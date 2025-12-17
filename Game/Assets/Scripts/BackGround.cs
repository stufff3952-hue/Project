using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();


        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;
        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;
        transform.localScale = new Vector3(
            worldScreenWidth / width,
            worldScreenHeight / height,
            1
        );
        transform.position = new Vector3(Camera.main.transform.position.x,
                                         Camera.main.transform.position.y,
                                         transform.position.z);
    }
}