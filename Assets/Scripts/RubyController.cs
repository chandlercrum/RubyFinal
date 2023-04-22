using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;

    public int maxHealth = 5;

    private int cogs;

    private int score;

    public static int level;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI cogsText;
    public GameObject winText;
    public GameObject loseText;
    public GameObject finalWinText;

    public GameObject projectilePrefab;

    public GameObject healthUpEffect;
    public GameObject healthDownEffect;

    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip fixSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip collectedClip;

    public GameObject BGM;

    public int health { get { return currentHealth; } }
    int currentHealth;

    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;

    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;

    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();

        cogs = 5;
        score = 0;
        level = 1;

        SetScoreText();
        winText.SetActive(false);
        loseText.SetActive(false);
        finalWinText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        cogsText.text = "Ammo: " + cogs.ToString();

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (cogs > 0)
            {
                Launch();
                cogs = cogs - 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                if (score == 5)
                {
                    SceneManager.LoadScene("Second");
                }
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    character.DisplayDialog();
                }
            }
        }

        if (SceneManager.GetActiveScene().name == "Second")
        {
            Destroy(winText);
        }

        if (Input.GetKey(KeyCode.R))
        {

            if (health <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (score == 6)
            {
                SceneManager.LoadScene("Main");
            }
        }

        if (currentHealth <= 0)
        {
            loseText.SetActive(true);
            speed = 0.0f;
            PlaySound(loseSound);
            Destroy(BGM);
        }

        SetScoreText();
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;
            Instantiate(healthDownEffect, rigidbody2d.position + Vector2.up * 0.05f, Quaternion.identity);
            PlaySound(hitSound);
        }

        if (amount > 0)
        {
            Instantiate(healthUpEffect, rigidbody2d.position + Vector2.up * 0.05f, Quaternion.identity);
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);

        SetScoreText();
    }

    public void ChangeScore(int scoreAmount)
    {
        score = score + 1;
        SetScoreText();
        PlaySound(fixSound);

        if (SceneManager.GetActiveScene().name == "Second" && score >= 6)
        {
            finalWinText.SetActive(true);
            PlaySound(winSound);
            Destroy(BGM);
        }
    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");

        PlaySound(throwSound);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void SetScoreText()
    {
        scoreText.text = "Robots Fixed: " + score.ToString();

        if (level == 1)
        {
            if (score == 5)
            {
                winText.SetActive(true);
            }
        }

        if (level == 2)
        {
            if (score == 6)
            {
                finalWinText.SetActive(true);
                PlaySound(winSound);
                Destroy(BGM);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.collider.tag == "AmmoPickup")
        {
            cogs = cogs + 4;
            cogsText.text = cogs.ToString();
            Destroy(collision.collider.gameObject);
            PlaySound(collectedClip);
            Instantiate(healthUpEffect, rigidbody2d.position + Vector2.up * 0.05f, Quaternion.identity);
        }
    }
}