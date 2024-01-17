using UltraFunGuns.Patches;
using UnityEngine;

namespace UltraFunGuns
{
    [UFGWeapon("VoxelHand", "Architect's Hand", 0, true, WeaponIconColor.Green, true)]
    public class VoxelHand : UltraFunGunBase
    {
        [Configgy.Configgable("UltraFunGuns/Weapons/Architects Hand")]
        private static float interactRange = 9.5f;

        [Configgy.Configgable("UltraFunGuns/Weapons/Architects Hand")]
        private static float placeBlockFaceOffset = 0.5f;

        [Configgy.Configgable("UltraFunGuns/Weapons/Architects Hand")]
        private static float inputHoldDelay = 0.3f;

        [Configgy.Configgable("UltraFunGuns/Weapons/Architects Hand")]
        private static float autoInteractInterval = 0.175f;

        private bool primaryPressed => InputManager.Instance.InputSource.Fire1.IsPressed && !om.paused;
        private bool secondaryPressed => InputManager.Instance.InputSource.Fire2.IsPressed && !om.paused;
        private bool middleClickPerformed => Input.GetKeyDown(KeyCode.Mouse2);

        private VoxelData lastVoxelData;
        private VoxelData currentVoxelData;
        public VoxelData CurrentVoxelData => currentVoxelData;

        private float secondaryHoldTime;
        private float primaryHoldTime;

        private float timeUntilAutoBreak;
        private float timeUntilAutoSecondary;

        private GameObject selectionVisual;
        private VoxelHandReferences parts;

        int anim_Equip = Animator.StringToHash("Equip");
        int anim_Place = Animator.StringToHash("Place");
        int anim_Punch = Animator.StringToHash("Punch");

        public override void OnAwakeFinished()
        {
            parts = GetComponent<VoxelHandReferences>();
        }

        private void Start()
        {
            SetHeldVoxel(null);
        }

        private void Update()
        {
            CheckForInteraction();
            HandleInput();
        }

        private void CheckForInteraction()
        {
            if (!Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit, interactRange, LayerMaskDefaults.Get(LMD.Environment)))
            {
                GetSelectionVisual().SetActive(false);
                return;
            }

            Vector3 samplePos = hit.point + -hit.normal * placeBlockFaceOffset;
            Vector3 snappedPosition = VoxelLocation.SnapToWorldGrid(samplePos);

            if (currentVoxelData != null)
            {
                GetSelectionVisual().SetActive(true);
            } else
            {
                GetSelectionVisual().SetActive(hit.collider.TryGetComponent<Voxel>(out Voxel voxel));
            }  

            GetSelectionVisual().transform.position = snappedPosition;
        }

        private void HandleInput()
        {
            HandleBlockBreak();
            HandleBlockPlace();
            HandleSwitchVoxel();
            HandlePickBlock();
            HandleDebugSerialization();
        }

        private void HandleBlockBreak()
        {
            if (primaryPressed)
            {
                if (primaryHoldTime == 0)
                {
                    BreakBlockAction();
                }

                if(primaryHoldTime > inputHoldDelay)
                {
                    if(timeUntilAutoBreak <= 0f)
                    {
                        BreakBlockAction();
                        timeUntilAutoBreak = autoInteractInterval;
                    }else
                    {
                        timeUntilAutoBreak -= Time.deltaTime;
                    }
                }

                primaryHoldTime += Time.deltaTime;
            }
            else
            {
                primaryHoldTime = 0f;
            }
        }

        private void HandleBlockPlace()
        {
            if (secondaryPressed)
            {
                if (secondaryHoldTime == 0)
                {
                    SecondaryAction();
                }

                if (secondaryHoldTime > inputHoldDelay)
                {
                    if (timeUntilAutoSecondary <= 0f)
                    {
                        SecondaryAction();
                        timeUntilAutoSecondary = autoInteractInterval;
                    }
                    else
                    {
                        timeUntilAutoSecondary -= Time.deltaTime;
                    }
                }

                secondaryHoldTime += Time.deltaTime;
            }
            else
            {
                secondaryHoldTime = 0f;
            }
        }

        private void SecondaryAction()
        {
            if (currentVoxelData == null)
                InteractAction();
            else
                PlaceBlockAction();
        }

        private void InteractAction()
        {
            animator?.Play(anim_Place, 0, 0);

            if (!Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit, interactRange, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
                return;

            if (hit.collider.IsColliderEnemy(out EnemyIdentifier eid))
            {
                Vector3 euler = eid.transform.eulerAngles;
                eid.transform.rotation = Quaternion.Euler(euler.x,euler.y+180f,euler.z);
                //banana peel sound
                return;
            }

            if (hit.collider.TryGetComponent<Voxel>(out Voxel voxel))
            {
                if (voxel.VoxelData != null)
                    if (voxel.VoxelData.Sound != null)
                        AudioSource.PlayClipAtPoint(voxel.VoxelData.Sound, voxel.transform.position);

                voxel.Interact();
                return;
            }
        }

        private void HandleDebugSerialization()
        {
            if(Input.GetKeyDown(KeyCode.PageDown))
            {
                VoxelWorld.SaveCurrentWorld();
                Debug.Log("Saved.");
            }

            if(Input.GetKeyDown(KeyCode.PageUp))
            {
                VoxelWorld.LoadWorld(VoxelSaveManager.LoadFromName("debug"));
                Debug.Log("Saved.");
            }

            if(Input.GetKeyDown(KeyCode.N))
            {
                BuildStructure();
            }
        }

        private void BuildStructure()
        {
            if (!TryGetLookLocation(out VoxelLocation location))
                return;

            MyVoxelHouse myHouse = new MyVoxelHouse();
            myHouse.Build(location.Coordinate, UnityEngine.Random.Range(0,260));
        }

        private bool TryGetLookLocation(out VoxelLocation location, bool invertNormal = false)
        {
            location = new VoxelLocation();

            if (!Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit, interactRange, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
                return false;

            location.Position = hit.point + ((hit.normal * placeBlockFaceOffset) * ((invertNormal) ? 1 : -1));
            return true;
        }

        private void HandlePickBlock()
        {
            if (!middleClickPerformed)
                return;

            if (!Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit, interactRange, LayerMaskDefaults.Get(LMD.Environment)))
                return;

            if (hit.collider.TryGetComponent<Voxel>(out Voxel voxel))
                if (voxel.VoxelData != null)
                    SetHeldVoxel(voxel.VoxelData);
        }

        private VoxelSelectionMenu palette;

        private void HandleSwitchVoxel()
        {
            if (Input.GetKeyDown(KeyCode.X))
                ToggleHand();

            if (!UFGInput.SecretButton.WasPeformed())
                return;

            if(palette == null)
            {
                palette = GameObject.Instantiate(VoxelSelectionMenu.VoxelSelectionMenuPrefab, InstanceUIComponents.Rect).GetComponent<VoxelSelectionMenu>();
                palette.SetVoxelHand(this);
            }

            if (palette == null)
                return;

            if (palette.IsOpen)
                return;

            palette.OpenMenu();
        }

        private void ToggleHand()
        {
            if (currentVoxelData != null)
                SetHeldVoxel(null);
            else if (lastVoxelData != null)
                SetHeldVoxel(lastVoxelData);
        }

        public void SetHeldVoxel(VoxelData voxel)
        {
            lastVoxelData = currentVoxelData;
            currentVoxelData = voxel;
            bool voxelNull = currentVoxelData == null;

            parts.HandObject.SetActive(voxelNull);
            parts.DisplayedCube.SetActive(!voxelNull);

            if(!voxelNull)
                parts.DisplayCubeRenderer.sharedMaterial = voxel.Material;

            if(lastVoxelData != voxel)
                animator?.Play(anim_Equip, 0, 0);
        }

        private void PlaceBlockAction()
        {
            if (currentVoxelData == null)
                return;

            if (!Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit, interactRange, LayerMaskDefaults.Get(LMD.Environment)))
                return;

            Vector3 samplePosition = hit.point + hit.normal * placeBlockFaceOffset;
            Vector3 gridPosition = VoxelLocation.SnapToWorldGrid(samplePosition);

            VoxelLocation location = new VoxelLocation(gridPosition);

            if (VoxelWorld.CheckVoxelCollision(gridPosition))
                return;

            Voxel placedVoxel = Voxel.Create(location, currentVoxelData);
            VoxelWorld.ReplaceVoxel(location, placedVoxel);

            if (currentVoxelData.Sound != null)
                AudioSource.PlayClipAtPoint(currentVoxelData.Sound, gridPosition);

            animator?.Play(anim_Place, 0, 0);
        }
        

        private GameObject GetSelectionVisual()
        {
            if (selectionVisual != null)
                return selectionVisual;

            float scale = VoxelWorld.WorldScale + 0.01f;

            GameObject newBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newBlock.name = $"VH_SELECTION";
            newBlock.layer = 24;
            newBlock.transform.localScale = Vector3.one * scale;

            Voxel newVoxel = newBlock.AddComponent<Voxel>();
            newVoxel.SetVoxelData(VoxelDatabase.Selection);

            newBlock.GetComponent<Collider>().enabled = false;
            newBlock.SetActive(false);
            selectionVisual = newBlock;

            return selectionVisual;
        }

        private void BreakBlockAction()
        {
            animator?.Play(anim_Punch, 0, 0);

            if (!Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit, interactRange, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
                return;
            
            if(hit.collider.IsColliderEnemy(out EnemyIdentifier eid))
            {
                eid.DeliverDamage(hit.collider.gameObject, -hit.normal*interactRange, hit.point, 0.2f, false, 1.2f, gameObject);
                return;
            }

            if(hit.collider.TryGetComponent<IParriable>(out IParriable parriable))
            {
                if (parriable.Parry(hit.point, mainCam.forward * interactRange))
                    return;
            }

            if(hit.collider.TryGetComponent<Voxel>(out Voxel voxel))
            {
                voxel.Break();
                return;
            }
        }

        public override string GetDebuggingText()
        {
            string debugMsg = "";

            if(currentVoxelData != null)
                debugMsg += $"Current: {currentVoxelData.DisplayName}";

            return debugMsg;
        }

        private void OnEnable()
        {
            animator?.Play(anim_Equip, 0, 0);
        }

        private void OnDisable()
        {
            GetSelectionVisual().SetActive(false);
        }

        private void OnDestroy()
        {
            if(selectionVisual != null)
                Destroy(selectionVisual);
        }

    }
}
