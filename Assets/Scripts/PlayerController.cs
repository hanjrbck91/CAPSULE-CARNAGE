using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class PlayerController : MonoBehaviourPunCallbacks,IDamageable
{
    // reference to get the method for reloading sound
    private SingleShotGun activeGun; // Reference to the active SingleShotGun script

    #region Shooting Cooltime variable
    bool canShoot = true;
    #endregion

    #region Animation Fields

    [SerializeField] Animator moveAnimator;
    Animator shootAnimator;

    #endregion

    #region private Fields

    // Mouse cursor control
    bool isCursorLocked = true;

    //For Healthbar ui display and sync with the damage
    [SerializeField] Image healthbarImage;
    // In order the delete the other player Canvas from our scene (otherwise it gonna mess up everythin)
    [SerializeField] GameObject ui;

    [SerializeField] GameObject cameraHolder;

    [SerializeField] float sprintSpeed, walkSpeed, jumpForce, smoothTime;

    [Space]
    [SerializeField] Item[] items;

    int itemIndex;
    int previousItemIndex = -1;

    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVeloctiy;
    Vector3 moveAmount;
    
    Rigidbody rb;

    PhotonView PV;

    [Header("Mouse Smoothness")]
    [SerializeField] float mouseSensitivity;
    [SerializeField] float smoothX = 0.0f;
    [SerializeField] float smoothY = 0.0f;
    [SerializeField] float horizontalSmoothTime = 0.1f;
    [SerializeField] float verticalSmoothTime = 0.1f;

    #region Health Fields

    const float maxHealth = 100f;
    float currentHealth = maxHealth;

    #endregion

    /// <summary>
    /// Reference of the PlayerManager in order to show the death()
    /// </summary>
    PlayerManager playerManager;

    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start()
    {
        // Mouse Lock 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (PV.IsMine)
        {
            EquipItem(0);
        }
        else
        {

            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(ui);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse Lock and UnLock
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorLocked = !isCursorLocked;
            SetCursorState();
        }

        if (!PV.IsMine) return;

        Look();
        Move();
        Jump();

        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            }
            else
            {
                EquipItem(itemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
            {
                EquipItem(items.Length - 1);
            }
            else
            {
                EquipItem(itemIndex - 1);
            }
        }

        if (canShoot && (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)))
        {
            Debug.Log("Shoot button pressed");

            // Disable shooting temporarily
            canShoot = false;
            // To set a cool time between shooting 
            StartCoroutine(ResetShootCooldown());

            items[itemIndex].Use();
            shootAnimator.SetTrigger("Shoot");

            // Call RPC to sync the trigger across the network
            photonView.RPC("SyncShootTrigger", RpcTarget.Others);

           
        }



        if (Input.GetKeyDown(KeyCode.R))
        {
           
                shootAnimator.SetTrigger("Reload");

            // Play reload sound of the active gun
                activeGun?.ReloadSound();
            

            // Call RPC to sync the trigger across the network
            photonView.RPC("SyncReloadTrigger", RpcTarget.Others);
            
        }

        // You will Die if you fall out of the world
        if (transform.position.y < -10f)
        {
            Die();
        }
    }

    /// <summary>
    /// To set and shoot cool time in order to prevent multiple shooting at a time
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetShootCooldown()
    {
        // Wait for a short duration before enabling shooting again
        yield return new WaitForSeconds(0.1f); // Adjust this duration as needed
        canShoot = true;
    }

    /// <summary>
    /// Animation syncing using PunRpc
    /// </summary>
    [PunRPC]
    void SyncShootTrigger()
    {
        // Set the trigger on all other clients
        shootAnimator.SetTrigger("Shoot");
    }

    [PunRPC]
    void SyncReloadTrigger()
    {
        // Set the trigger on all other clients
        shootAnimator.SetTrigger("Reload");
    }
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Smoothing mouse input separately for horizontal and vertical movements
        mouseX = Mathf.SmoothDamp(mouseX, mouseX, ref smoothX, horizontalSmoothTime);
        mouseY = Mathf.SmoothDamp(mouseY, mouseY, ref smoothY, verticalSmoothTime);


        // Update total rotation
        verticalLookRotation += mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60f, 60f);

        // Rotate the player object around its y-axis based on horizontal mouse movement
        transform.Rotate(Vector3.up * mouseX);

        // Smoothly rotate the camera holder based on vertical mouse movement
        Quaternion targetRotation = Quaternion.Euler(Vector3.left * verticalLookRotation);
        cameraHolder.transform.localRotation = Quaternion.Slerp(cameraHolder.transform.localRotation, targetRotation, smoothTime * Time.deltaTime);
    }


    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVeloctiy, smoothTime);

        // Calculate the magnitude of moveAmount to determine movement speed
        float moveSpeed = moveAmount.magnitude;

        // Set the "isRunning" parameter in the animator based on moveSpeed
        moveAnimator.SetFloat("isRunning", moveSpeed);

        // Update the same parameter over the network
        photonView.RPC("UpdateMovementParameter", RpcTarget.Others, moveSpeed);
    }

    [PunRPC]
    void UpdateMovementParameter(float movement)
    {
        // Set the float parameter received from the network
        moveAnimator.SetFloat("isRunning", movement);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    void EquipItem(int _index)
    {
        if(_index == previousItemIndex)
        {
            return;
        }

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);
        shootAnimator = items[itemIndex].itemGameObject.GetComponent<Animator>();

        // Get the SingleShotGun script of the currently active gun
        activeGun = items[itemIndex].GetComponent<SingleShotGun>();
        // Debug statement to verify activeGun assignment
        Debug.Log("Active gun: " + activeGun);



        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Runs on the shooter's Computer
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage)
    {
        PV.RPC(nameof(RPC_TakeDamage), PV.Owner,damage);
    }

    /// <summary>
    ///  Runs's on everybody's computer, but PV.IsMine check makes it only run on the victim's Computer
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    void RPC_TakeDamage(float damage , PhotonMessageInfo info)
    {

        Debug.Log("took damage" + damage);

        currentHealth -= damage;

        // We are taking the percentage of the Health 
        healthbarImage.fillAmount = currentHealth / maxHealth;

        if(currentHealth <=0)
        {
            Die();
            PlayerManager.Find(info.Sender).GetKill();
           
        }

    }


    void Die()
    {
        playerManager.Die();
    }

    #region Mouse Cursor Lock/Unlock
    void SetCursorState()
    {
        if (isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #endregion

}
