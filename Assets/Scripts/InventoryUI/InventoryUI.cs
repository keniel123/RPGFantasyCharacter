using RPGController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGController
{
    public class InventoryUI : MonoBehaviour
    {
        public static InventoryUI inventoryUI;
        InputUI inputUI;

        public EquipmentLeft equipment_Left;
        public CenterOverlay center_Overlay;
        public WeaponInfo weaponInfo;
        public PlayerStatus playerStatus;
        public Transform playerStatusGrid;
        public GameObject gameMenu, inventory, centerMain, centerRight, centerOverlay;
        public GameObject equipmentPanel;
        public GameObject inventoryPanel;
        public GameObject gameUI;
        public UIState currentState;
        public EquipmentSlotsUI equipmentSlotsUI;
        public Transform equipmentSlotParent;
        //List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();
        EquipmentSlot[,] equipmentSlots;
        public Vector2 currentSlotPos;

        List<IconBase> iconSlotsCreated = new List<IconBase>();
        public int currentInv_Index;
        List<IconBase> currentCreatedItems;
        int maxInv_Index;
        int previousInv_Index;
        IconBase currentInvIcon;

        float inputT;
        bool dontMove;

        public Color slotSelectedColor;
        public Color slotUnSelectedColor;
        EquipmentSlot currentEqSlot;
        EquipmentSlot prevEqSlot;

        float inputTimer;
        public float inputDelay = 0.4f;

        public bool load;
        public InventoryManager invManager;
        public bool isSwitching;

        private void Awake()
        {
            inventoryUI = this;
        }

        #region Initialization
        public void Init(InventoryManager inventoryManager)
        {
            invManager = inventoryManager;
            inputUI = InputUI.Instance;
            CreateUIElements();
            InitEquipmentSlots();
        }

        void CreateUIElements()
        {

            InitWeaponInfo();
            InitPlayerStatus();
            InitWeaponStatus();
        }

        void InitWeaponInfo()
        {

            //Attack Powers
            for (int i = 0; i < 6; i++)
            {
                CreateAttackDefenseUIElement(weaponInfo.attackPowerSlots, weaponInfo.attackPowerGrid, (AttackDefenseType)i);
            }

            //Guard absroptions
            for (int i = 0; i < 5; i++)
            {
                CreateAttackDefenseUIElement(weaponInfo.guardAbsorptions, weaponInfo.guardAbsorptionGrid, (AttackDefenseType)i);
            }

            CreateAttackDefenseUIElement(weaponInfo.guardAbsorptions, weaponInfo.guardAbsorptionGrid, AttackDefenseType.Stability);

            //Additional Effects
            CreateAttackDefenseUIElementMini(weaponInfo.additionalEffects, weaponInfo.additionalEffGrid, AttackDefenseType.Bleed);
            CreateAttackDefenseUIElementMini(weaponInfo.additionalEffects, weaponInfo.additionalEffGrid, AttackDefenseType.Curse);
            CreateAttackDefenseUIElementMini(weaponInfo.additionalEffects, weaponInfo.additionalEffGrid, AttackDefenseType.Frost);

            //Attack Bonuses
            CreateAttributeUIElementMini(weaponInfo.attackBonuses, weaponInfo.attackBonusesGrid, AttributeType.Strength);
            CreateAttributeUIElementMini(weaponInfo.attackBonuses, weaponInfo.attackBonusesGrid, AttributeType.Dexterity);
            CreateAttributeUIElementMini(weaponInfo.attackBonuses, weaponInfo.attackBonusesGrid, AttributeType.Intelligence);
            CreateAttributeUIElementMini(weaponInfo.attackBonuses, weaponInfo.attackBonusesGrid, AttributeType.Faith);

            //Attack Requirements
            CreateAttributeUIElementMini(weaponInfo.attackRequirements, weaponInfo.attackRequirementsGrid, AttributeType.Strength);
            CreateAttributeUIElementMini(weaponInfo.attackRequirements, weaponInfo.attackRequirementsGrid, AttributeType.Dexterity);
            CreateAttributeUIElementMini(weaponInfo.attackRequirements, weaponInfo.attackRequirementsGrid, AttributeType.Intelligence);
            CreateAttributeUIElementMini(weaponInfo.attackRequirements, weaponInfo.attackRequirementsGrid, AttributeType.Faith);
        }

        void InitPlayerStatus()
        {

            CreateAttributeUIElement(playerStatus.attributeSlots, playerStatus.attributeGrid, AttributeType.Level);
            CreateEmptySlot(playerStatus.attributeGrid);

            for (int i = 1; i < 10; i++)
            {
                CreateAttributeUIElement(playerStatus.attributeSlots, playerStatus.attributeGrid, (AttributeType)i);
            }

            CreateEmptySlot(playerStatus.attributeGrid);
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                index += 10;
                CreateAttributeUIElement(playerStatus.attributeSlots, playerStatus.attributeGrid, (AttributeType)index);
            }

            CreateEmptySlot(playerStatus.attributeGrid);
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                index += 13;
                CreateAttributeUIElement(playerStatus.attributeSlots, playerStatus.attributeGrid, (AttributeType)index);
            }
        }

        void InitWeaponStatus()
        {

            CreateWeaponStatusSlot(playerStatus.defenseSlots, playerStatus.defenseAbsorptionGrid, AttackDefenseType.Physical);
            CreateWeaponStatusSlot(playerStatus.defenseSlots, playerStatus.defenseAbsorptionGrid, AttackDefenseType.Strike, "VS Strike");
            CreateWeaponStatusSlot(playerStatus.defenseSlots, playerStatus.defenseAbsorptionGrid, AttackDefenseType.Slash, "VS Slash");
            CreateWeaponStatusSlot(playerStatus.defenseSlots, playerStatus.defenseAbsorptionGrid, AttackDefenseType.Thrust, "VS Thrust");

            CreateWeaponStatusSlot(playerStatus.defenseSlots, playerStatus.defenseAbsorptionGrid, AttackDefenseType.Magic);
            CreateWeaponStatusSlot(playerStatus.defenseSlots, playerStatus.defenseAbsorptionGrid, AttackDefenseType.Fire);
            CreateWeaponStatusSlot(playerStatus.defenseSlots, playerStatus.defenseAbsorptionGrid, AttackDefenseType.Lightning);
            CreateWeaponStatusSlot(playerStatus.defenseSlots, playerStatus.defenseAbsorptionGrid, AttackDefenseType.Dark);

            CreateWeaponStatusSlot(playerStatus.resistanceSlots, playerStatus.resistanceArmorGrid, AttackDefenseType.Bleed);
            CreateWeaponStatusSlot(playerStatus.resistanceSlots, playerStatus.resistanceArmorGrid, AttackDefenseType.Poison);
            CreateWeaponStatusSlot(playerStatus.resistanceSlots, playerStatus.resistanceArmorGrid, AttackDefenseType.Frost);
            CreateWeaponStatusSlot(playerStatus.resistanceSlots, playerStatus.resistanceArmorGrid, AttackDefenseType.Curse);

            CreateAttackPowerSlot(playerStatus.attackPowerSlots, playerStatus.attackPowerGrid, "R Weapon 1");
            CreateAttackPowerSlot(playerStatus.attackPowerSlots, playerStatus.attackPowerGrid, "R Weapon 2");
            CreateAttackPowerSlot(playerStatus.attackPowerSlots, playerStatus.attackPowerGrid, "R Weapon 3");

            CreateAttackPowerSlot(playerStatus.attackPowerSlots, playerStatus.attackPowerGrid, "L Weapon 1");
            CreateAttackPowerSlot(playerStatus.attackPowerSlots, playerStatus.attackPowerGrid, "L Weapon 2");
            CreateAttackPowerSlot(playerStatus.attackPowerSlots, playerStatus.attackPowerGrid, "L Weapon 3");
        }

        void InitEquipmentSlots()
        {
            EquipmentSlot[] eq = equipmentSlotParent.GetComponentsInChildren<EquipmentSlot>();
            equipmentSlots = new EquipmentSlot[5, 6];

            for (int i = 0; i < eq.Length; i++)
            {
                int x = Mathf.RoundToInt(eq[i].slotPos.x);
                int y = Mathf.RoundToInt(eq[i].slotPos.y);
                equipmentSlots[x, y] = eq[i];
                eq[i].Init(this);
            }
        }

        #endregion

        #region Create Status UI Elements

        void CreateAttackDefenseUIElement(List<AttackDefenseSlot> list, Transform parent, AttackDefenseType type)
        {
            AttackDefenseSlot attackDefense = new AttackDefenseSlot();
            attackDefense.type = type;
            list.Add(attackDefense);

            GameObject go = Instantiate(weaponInfo.slotTemplate) as GameObject;
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;

            attackDefense.slot = go.GetComponent<InventoryUISlot>();
            attackDefense.slot.text1.text = attackDefense.type.ToString();
            attackDefense.slot.text2.text = "20";
            go.SetActive(true);

        }

        //Creating for additional effects
        void CreateAttackDefenseUIElementMini(List<AttackDefenseSlot> list, Transform parent, AttackDefenseType type)
        {
            AttackDefenseSlot attackDefense = new AttackDefenseSlot();
            attackDefense.type = type;
            list.Add(attackDefense);

            GameObject go = Instantiate(weaponInfo.slotMini) as GameObject;
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;

            attackDefense.slot = go.GetComponent<InventoryUISlot>();
            attackDefense.slot.text1.text = "-";
            go.SetActive(true);
        }

        //Creating for additional effects
        void CreateAttributeUIElementMini(List<AttributeSlot> list, Transform parent, AttributeType type)
        {
            AttributeSlot attackDefense = new AttributeSlot();
            attackDefense.type = type;
            list.Add(attackDefense);

            GameObject go = Instantiate(weaponInfo.slotMini) as GameObject;
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;

            attackDefense.slot = go.GetComponent<InventoryUISlot>();
            attackDefense.slot.text1.text = "-";
            go.SetActive(true);
        }

        //Creating for player status elements
        void CreateAttributeUIElement(List<AttributeSlot> list, Transform parent, AttributeType type, string txt1Text = null)
        {
            AttributeSlot attackDefense = new AttributeSlot();
            attackDefense.type = type;
            list.Add(attackDefense);

            GameObject go = Instantiate(playerStatus.playerStatus_Slot_Template) as GameObject;
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;

            attackDefense.slot = go.GetComponent<InventoryUISlot>();
            if (string.IsNullOrEmpty(txt1Text))
            {
                attackDefense.slot.text1.text = type.ToString().Replace('_', ' ');
            }
            else
            {
                attackDefense.slot.text1.text = txt1Text;
            }

            attackDefense.slot.text2.text = "30";
            go.SetActive(true);
        }

        //Creating for blank row
        void CreateEmptySlot(Transform parent)
        {
            GameObject go = Instantiate(playerStatus.emptySlot) as GameObject;
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;
            go.SetActive(true);
        }

        void CreateWeaponStatusSlot(List<PlayerStatusDef> list, Transform parent, AttackDefenseType type, string txt1Text = null)
        {
            PlayerStatusDef weapon = new PlayerStatusDef();
            GameObject go = Instantiate(playerStatus.playerStatus_DoubleSlot_Template) as GameObject;
            go.transform.SetParent(parent);
            go.SetActive(true);

            weapon.type = type;
            weapon.slot = go.GetComponent<InventoryUIDoubleSlot>();
            if (string.IsNullOrEmpty(txt1Text))
            {
                weapon.slot.text1.text = type.ToString().Replace('_', ' ');
            }
            else
            {
                weapon.slot.text1.text = txt1Text;
            }

            weapon.slot.text2.text = "30";
            weapon.slot.text3.text = "30";
        }

        void CreateAttackPowerSlot(List<AttackPowerSlot> list, Transform parent, string id)
        {
            AttackPowerSlot attackPower = new AttackPowerSlot();
            list.Add(attackPower);

            GameObject go = Instantiate(weaponInfo.slotTemplate) as GameObject;
            go.transform.SetParent(parent);
            attackPower.slot = go.GetComponent<InventoryUISlot>();
            attackPower.slot.text1.text = id;
            attackPower.slot.text2.text = "30";
            go.SetActive(true);

        }

        #endregion

        #region Loading Items
        /// <summary>
        ///Load items to inventory UI in Left Equipment Panel
        /// </summary>
        /// <param name="item">Item Type</param>
        public void LoadCurrentItems(Itemtype item)
        {
            List<Item> itemList = SessionManager.Instance.GetItemsAsList(item);

            if (itemList == null || itemList.Count == 0)
            {
                return;
            }

            GameObject prefabSlot = equipment_Left.leftInventory.inventorySlotTemplate;
            Transform parent = equipment_Left.leftInventory.slotGrid.transform;

            int diff = iconSlotsCreated.Count - itemList.Count;
            int extra = (diff > 0) ? diff : 0;

            maxInv_Index = itemList.Count;
            currentCreatedItems = new List<IconBase>();

            //By default, set currently selected item the first one in the inventory list
            currentInv_Index = 0;

            for (int i = 0; i < itemList.Count + extra; i++)
            {
                if (i > itemList.Count - 1)
                {
                    iconSlotsCreated[i].gameObject.SetActive(false);
                    continue;
                }

                IconBase icon = null;
                if (iconSlotsCreated.Count - 1 < i)
                {
                    GameObject go = Instantiate(prefabSlot) as GameObject;
                    go.transform.SetParent(parent);
                    go.SetActive(true);
                    icon = go.GetComponent<IconBase>();
                    iconSlotsCreated.Add(icon);
                }
                else
                {
                    icon = iconSlotsCreated[i];
                }

                currentCreatedItems.Add(icon);
                icon.gameObject.SetActive(true);
                icon.icon.enabled = true;
                icon.icon.sprite = itemList[i].itemIcon;
                icon.id = itemList[i].item_id;
            }
        }

        #endregion

        Itemtype ItemTypeFromSlotType(EquipmentSlotType eqType)
        {
            switch (eqType)
            {
                case EquipmentSlotType.Weapons:
                    return Itemtype.Weapon;
                case EquipmentSlotType.Arrows:
                case EquipmentSlotType.Bolts:
                case EquipmentSlotType.Equipment:
                    return Itemtype.Equipment;
                case EquipmentSlotType.Rings:
                case EquipmentSlotType.Covenant:
                case EquipmentSlotType.Consumables:
                    return Itemtype.Consumable;
                default:
                    return Itemtype.Spell;
            }
        }


        void HandleSlotInput(InputUI inputUI)
        {
            if (currentEqSlot == null)
            {
                return;
            }

            if (inputUI.lt_input)
            {
                isSwitching = !isSwitching;
                if (isSwitching)
                {
                    LoadCurrentItems(ItemTypeFromSlotType(currentEqSlot.eqSlotType));
                }
                else
                {
                    Itemtype type = ItemTypeFromSlotType(currentEqSlot.eqSlotType);
                    if (type == Itemtype.Weapon)
                    {
                        int targetIndex = currentEqSlot.itemPosition;

                        //If slot index is greater than 2, it is left hand weapon,
                        //since first 3 slots belong to right hand weapons
                        bool isLeft = (currentEqSlot.itemPosition > 2) ? true : false ;
                        if (isLeft)
                        {
                            targetIndex -= 3;
                            invManager.leftHandWeapons[targetIndex] = currentInvIcon.id;
                        }
                        else
                        {
                            invManager.rightHandWeapons[targetIndex] = currentInvIcon.id;
                        }
                    }
                    else
                    {
                        invManager.consumableItems[currentEqSlot.itemPosition] = currentInvIcon.id;
                    }

                    LoadEquipment(invManager,true);
                }

                ChangeToSwitching();
            }

            if (inputUI.b_input)
            {
                isSwitching = false;
                ChangeToSwitching();
            }
        }
        
        void HandleSlotMovement(InputUI inputUI)
        {
            int x = Mathf.RoundToInt(currentSlotPos.x);
            int y = Mathf.RoundToInt(currentSlotPos.y);

            bool up = (inputUI.vertical > 0);
            bool down = (inputUI.vertical < 0);

            bool left = (inputUI.horizontal < 0);
            bool right = (inputUI.horizontal > 0);

            if (!up && !down && !left && !right)
            {
                inputTimer = 0;
            }
            else
            {
                inputTimer -= Time.deltaTime;
            }

            if (inputTimer < 0)
            {
                inputTimer = 0;
            }

            if (inputTimer > 0)
            {
                return;
            }

            if (up)
            {
                y--;
                inputTimer = inputDelay;
            }

            if (down)
            {
                y++;
                inputTimer = inputDelay;
            }

            if (left)
            {
                x--;
                inputTimer = inputDelay;
            }

            if (right)
            {
                x++;
                inputTimer = inputDelay;
            }

            if (x > 4)
            {
                x = 0;
            }

            if (x < 0)
            {
                x = 0;
            }

            if (y > 5)
            {
                y = 0;
            }
            if (y < 0)
            {
                y = 5;
            }

            if (currentEqSlot != null)
            {
                currentEqSlot.iconBase.background.color = slotUnSelectedColor;
            }

            if (x == 4 && y == 3)
            {
                x = 4;
                y = 2;
            }

            currentEqSlot = equipmentSlots[x, y];
            currentSlotPos.x = x;
            currentSlotPos.y = y;
            if (currentEqSlot != null)
            {
                currentEqSlot.iconBase.background.color = slotSelectedColor;
            }
        }

        void HandleInventoryMovement(InputUI inputHandler)
        {
            int x = Mathf.RoundToInt(currentSlotPos.x);
            int y = Mathf.RoundToInt(currentSlotPos.y);

            bool up = (inputUI.vertical > 0);
            bool down = (inputUI.vertical < 0);

            bool left = (inputUI.horizontal < 0);
            bool right = (inputUI.horizontal > 0);

            if (!up && !down && !left && !right)
            {
                inputTimer = 0;
            }
            else
            {
                inputTimer -= Time.deltaTime;
            }

            if (inputTimer < 0)
            {
                inputTimer = 0;
            }

            if (inputTimer > 0)
            {
                return;
            }

            if (up)
            {
                //Inventory has fixed column count 5
                currentInv_Index -= 5;
                inputTimer = inputDelay;
            }

            if (down)
            {
                //Inventory has fixed column count 5
                currentInv_Index += 5;
                inputTimer = inputDelay;
            }

            if (left)
            {
                currentInv_Index -= 1;
                inputTimer = inputDelay;
            }

            if (right)
            {
                currentInv_Index += 1;
                inputTimer = inputDelay;
            }

            //Clamp
            //If you reach at the end, go to start, first element
            if (currentInv_Index > maxInv_Index - 1)
            {
                currentInv_Index = 0;
            }

            if (currentInv_Index < 0)
            {
                currentInv_Index = 0;
            }
        }

        void HandleUIState(InputUI inputHandler)
        {
            switch (currentState)
            {
                case UIState.equipment:
                    if (!isSwitching)
                    {
                        HandleSlotMovement(inputHandler);
                    }
                    else
                    {
                        HandleInventoryMovement(inputHandler);
                    }
                    HandleSlotInput(inputHandler);
                    break;
                case UIState.inventory:
                    HandleInventoryMovement(inputHandler);
                    break;
                case UIState.attributes:
                    break;
                case UIState.messages:
                    break;
                case UIState.options:
                    break;
                default:
                    break;
            }
        }

        void ChangeToSwitching()
        {
            if (isSwitching)
            {
                equipmentPanel.SetActive(false);
                inventoryPanel.SetActive(true);
            }
            else
            {
                equipmentPanel.SetActive(true);
                inventoryPanel.SetActive(false);
            }
        }

        public void OpenUI()
        {
            LoadEquipment(invManager);
            gameMenu.SetActive(false);
            inventory.SetActive(true);
            gameUI.SetActive(false);
            prevEqSlot = null;
            currentInv_Index = -1;
        }

        public void CloseUI()
        {
            gameMenu.SetActive(false);
            inventory.SetActive(false);
            gameUI.SetActive(true);
            prevEqSlot = null;
            currentInv_Index = -1;

        }

        public void Tick()
        {
            inputUI.Tick();
            HandleUIState(inputUI);

            if (prevEqSlot != currentEqSlot)
            {
                if (currentEqSlot != null)
                {
                    equipment_Left.slotName.text = currentEqSlot.slotName;
                    LoadItemFromSlot(currentEqSlot.iconBase);
                }

            }

            if (currentCreatedItems != null)
            {
                if (currentCreatedItems.Count > 0)
                {
                    if (previousInv_Index != currentInv_Index)
                    {
                        if (currentInvIcon)
                        {
                            currentInvIcon.background.color = slotUnSelectedColor;
                        }
                        if (currentInv_Index < currentCreatedItems.Count)
                        {
                            currentInvIcon = currentCreatedItems[currentInv_Index];
                            currentInvIcon.background.color = slotSelectedColor;
                            LoadItemFromSlot(currentInvIcon);
                        }

                    }
                }
            }

            prevEqSlot = currentEqSlot;
            previousInv_Index = currentInv_Index;
        }

        public void LoadEquipment(InventoryManager inv, bool loadOnCharacter = false)
        {
            //Load Right Hand weapons
            for (int i = 0; i < inv.rightHandWeapons.Count; i++)
            {
                //Clamp = no more than 3 items (thats how much slot we have for RightHand Weapons)
                if (i > 2)
                    break;

                EquipmentSlot eqSlot = equipmentSlotsUI.weaponSlots[i];
                equipmentSlotsUI.UpdateEquipmentSlot(inv.rightHandWeapons[i], eqSlot, Itemtype.Weapon);
                eqSlot.itemPosition = i;
            }


            //Load Left Hand weapons
            for (int i = 0; i < inv.leftHandWeapons.Count; i++)
            {
                //Clamp = no more than 3 items (thats how much slot we have for RightHand Weapons)
                if (i > 2)
                    break;

                //i+3 -- > Left side index starts after 3 slots of right hand weapons
                EquipmentSlot eqSlot = equipmentSlotsUI.weaponSlots[i + 3];
                equipmentSlotsUI.UpdateEquipmentSlot(inv.leftHandWeapons[i], eqSlot, Itemtype.Weapon);
                eqSlot.itemPosition = i + 3;

            }

            //Load Consmable items
            for (int i = 0; i < inv.consumableItems.Count; i++)
            {
                //Clamp = no more than 3 items (thats how much slot we have for RightHand Weapons)
                if (i > 9)
                    break;

                EquipmentSlot eqSlot = equipmentSlotsUI.consumableSlots[i];
                equipmentSlotsUI.UpdateEquipmentSlot(inv.consumableItems[i], eqSlot, Itemtype.Consumable);
            }

            if (loadOnCharacter)
            {
                invManager.LoadInventory();
            }
        }

        void LoadItemFromSlot(IconBase iconBase)
        {
            if (string.IsNullOrEmpty(iconBase.id))
            {
                iconBase.id = "Unarmed";
            }
            
            ResourcesManager resourceManager = ResourcesManager.Instance;

            switch (currentEqSlot.eqSlotType)
            {
                case EquipmentSlotType.Weapons:
                    LoadWeaponItem(resourceManager, iconBase);
                    break;
                case EquipmentSlotType.Arrows:
                    break;
                case EquipmentSlotType.Bolts:
                    break;
                case EquipmentSlotType.Equipment:
                    break;
                case EquipmentSlotType.Rings:
                    break;
                case EquipmentSlotType.Covenant:
                    break;
                case EquipmentSlotType.Consumables:
                    LoadConsumableItem(resourceManager);
                    break;
                default:
                    break;
            }

        }

        void LoadWeaponItem(ResourcesManager resManager, IconBase icon)
        {
            string weaponID = icon.id;
            WeaponStats weaponStats = resManager.GetWeaponStats(weaponID);
            Item item = resManager.GetItem(icon.id, Itemtype.Weapon);
            equipment_Left.currentItem.text = item.name_item;

            //Update Center Overlay UI Panel
            UpdateCenterOverlay(item);
            center_Overlay.skillName.text = weaponStats.skillName;

            //Center Main UI
            weaponInfo.smallIcon.sprite = item.itemIcon;
            weaponInfo.itemName.text = item.name_item;
            weaponInfo.weaponType.text = weaponStats.weaponType;
            weaponInfo.damageType.text = weaponStats.damageType;
            weaponInfo.skillName.text = weaponStats.skillName;
            weaponInfo.weightCost.text = weaponStats.weightCost.ToString();

            //Update the min durability!!!
            weaponInfo.durability_Min.text = weaponInfo.durability_Max.ToString();
            weaponInfo.durability_Max.text = weaponInfo.durability_Max.ToString();

            //Attack Power
            UpdateAttackDefenseUIElement(weaponInfo.attackPowerSlots, AttackDefenseType.Physical, weaponStats.attackPhysical.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.attackPowerSlots, AttackDefenseType.Magic, weaponStats.attackMagic.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.attackPowerSlots, AttackDefenseType.Fire, weaponStats.attackFire.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.attackPowerSlots, AttackDefenseType.Lightning, weaponStats.attackLigthning.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.attackPowerSlots, AttackDefenseType.Dark, weaponStats.attackDark.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.attackPowerSlots, AttackDefenseType.Critical, weaponStats.attackCritical.ToString());

            UpdateAttackDefenseUIElement(weaponInfo.additionalEffects, AttackDefenseType.Frost, weaponStats.attackFrost.ToString(), true);
            UpdateAttackDefenseUIElement(weaponInfo.additionalEffects, AttackDefenseType.Curse, weaponStats.attackCurse.ToString(), true);
            //UpdateAttackDefenseUIElement(weaponInfo.additionalEffects, AttackDefenseType.Poison, weaponStats.poisonDamage.ToString());
            //UpdateAttackDefenseUIElement(weaponInfo.additionalEffects, AttackDefenseType.Bleed, weaponStats.frostDamage.ToString(), true);

            //Guard Absorptions
            UpdateAttackDefenseUIElement(weaponInfo.guardAbsorptions, AttackDefenseType.Physical, weaponStats.defensePhysical.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.guardAbsorptions, AttackDefenseType.Magic, weaponStats.defenseMagic.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.guardAbsorptions, AttackDefenseType.Fire, weaponStats.defenseFire.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.guardAbsorptions, AttackDefenseType.Lightning, weaponStats.defenseLigthning.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.guardAbsorptions, AttackDefenseType.Dark, weaponStats.defenseDark.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.guardAbsorptions, AttackDefenseType.Dark, weaponStats.defenseDark.ToString());
            UpdateAttackDefenseUIElement(weaponInfo.guardAbsorptions, AttackDefenseType.Stability, weaponStats.defenseStability.ToString());
        }

        void LoadConsumableItem(ResourcesManager resManager)
        {

            string consumableID = currentEqSlot.iconBase.id;
            Item item = resManager.GetItem(currentEqSlot.iconBase.id, Itemtype.Consumable);

            UpdateCenterOverlay(item);
        }

        void UpdateCenterOverlay(Item item)
        {

            //Center Overlay UI
            center_Overlay.largeWeaponIcon.sprite = item.itemIcon;
            center_Overlay.itemName.text = item.name_item;
            center_Overlay.itemDescriptipn.text = item.itemDescription;
            center_Overlay.skillDescription.text = item.skillDescription;

        }

        void UpdateAttackDefenseUIElement(List<AttackDefenseSlot> list, AttackDefenseType type, string value, bool onTxt1 = false)
        {
            AttackDefenseSlot attackDefSlot = weaponInfo.GetAttackDefenseSlot(list, type);

            if (!onTxt1)
            {
                attackDefSlot.slot.text2.text = value;
            }
            else
            {
                attackDefSlot.slot.text1.text = value;
            }
        }
    }

    public enum EquipmentSlotType
    {
        Weapons,
        Arrows,
        Bolts,
        Equipment,
        Rings,
        Covenant,
        Consumables
    }

    public enum UIState
    {
        equipment, inventory, attributes, messages, options
    }

    [System.Serializable]
    public class EquipmentSlotsUI
    {
        public List<EquipmentSlot> weaponSlots = new List<EquipmentSlot>();
        public List<EquipmentSlot> arrowSlots = new List<EquipmentSlot>();
        public List<EquipmentSlot> boltSlots = new List<EquipmentSlot>();
        public List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();
        public List<EquipmentSlot> ringSlots = new List<EquipmentSlot>();
        public List<EquipmentSlot> consumableSlots = new List<EquipmentSlot>();
        public EquipmentSlot covenantSlot;

        public void UpdateEquipmentSlot(string itemId, EquipmentSlot eqSlot, Itemtype itemType)
        {
            Item item = ResourcesManager.Instance.GetItem(itemId, itemType);
            eqSlot.iconBase.icon.sprite = item.itemIcon;
            eqSlot.iconBase.icon.enabled = true;
            eqSlot.iconBase.id = item.item_id;
        }

        public void AddSlotOnList(EquipmentSlot eqSlot)
        {
            switch (eqSlot.eqSlotType)
            {
                case EquipmentSlotType.Weapons:
                    weaponSlots.Add(eqSlot);
                    break;
                case EquipmentSlotType.Arrows:
                    arrowSlots.Add(eqSlot);
                    break;
                case EquipmentSlotType.Bolts:
                    boltSlots.Add(eqSlot);
                    break;
                case EquipmentSlotType.Equipment:
                    equipmentSlots.Add(eqSlot);
                    break;
                case EquipmentSlotType.Rings:
                    ringSlots.Add(eqSlot);
                    break;
                case EquipmentSlotType.Covenant:
                    covenantSlot = eqSlot;
                    break;
                case EquipmentSlotType.Consumables:
                    consumableSlots.Add(eqSlot);
                    break;
                default:
                    break;
            }
        }

    }

    [System.Serializable]
    public class EquipmentLeft
    {
        public Text slotName;
        public Text currentItem;
        public LeftInventory leftInventory;
    }

    [System.Serializable]
    public class PlayerStatus
    {
        public GameObject playerStatus_Slot_Template;
        public GameObject playerStatus_DoubleSlot_Template;
        public GameObject emptySlot;

        public Transform attributeGrid;
        public Transform attackPowerGrid;
        public Transform defenseAbsorptionGrid;
        public Transform resistanceArmorGrid;

        public List<AttributeSlot> attributeSlots = new List<AttributeSlot>();
        public List<AttackPowerSlot> attackPowerSlots = new List<AttackPowerSlot>();
        public List<PlayerStatusDef> defenseSlots = new List<PlayerStatusDef>();
        public List<PlayerStatusDef> resistanceSlots = new List<PlayerStatusDef>();

    }

    [System.Serializable]
    public class PlayerStatusDef
    {
        public AttackDefenseType type;
        public InventoryUIDoubleSlot slot;
    }

    [System.Serializable]
    public class LeftInventory
    {
        public Slider inventorySlider;
        public GameObject inventorySlotTemplate;
        public GameObject slotGrid;
    }

    [System.Serializable]
    public class CenterOverlay
    {
        public Image largeWeaponIcon;
        public Text itemName;
        public Text itemDescriptipn;
        public Text skillName;
        public Text skillDescription;
    }

    #region WeaponInfo
    [System.Serializable]
    public class WeaponInfo
    {

        public Image smallIcon;
        public GameObject slotTemplate;
        public GameObject slotMini;
        public GameObject breakSlot;
        public Text itemName;
        public Text weaponType;
        public Text damageType;
        public Text skillName;
        public Text manaCost;
        public Text weightCost;
        public Text durability_Min;
        public Text durability_Max;

        //Attack Power Slots
        public List<AttackDefenseSlot> attackPowerSlots = new List<AttackDefenseSlot>();
        public Transform attackPowerGrid;

        //Guard Absorption
        public List<AttackDefenseSlot> guardAbsorptions = new List<AttackDefenseSlot>();
        public Transform guardAbsorptionGrid;

        //Additional Effects
        public List<AttackDefenseSlot> additionalEffects = new List<AttackDefenseSlot>();
        public Transform additionalEffGrid;

        //Attack Bonuses
        public List<AttributeSlot> attackBonuses = new List<AttributeSlot>();
        public Transform attackBonusesGrid;

        //Attack Requirements
        public List<AttributeSlot> attackRequirements = new List<AttributeSlot>();
        public Transform attackRequirementsGrid;

        public AttributeSlot GetAttribute(List<AttributeSlot> list, AttributeType attributeType)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].type == attributeType)
                {
                    return list[i];
                }
            }

            return null;
        }

        public AttackDefenseSlot GetAttackDefenseSlot(List<AttackDefenseSlot> list, AttackDefenseType attributeType)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].type == attributeType)
                {
                    return list[i];
                }
            }

            return null;
        }
    }

    //Attack Power Slot
    [System.Serializable]
    public class AttributeSlot
    {

        public bool isBreak;
        public AttributeType type;
        public InventoryUISlot slot;
    }

    [System.Serializable]
    public class AttackPowerSlot
    {
        public InventoryUISlot slot;
    }

    [System.Serializable]
    public class AttackDefenseSlot
    {
        public bool isBreak;
        public AttackDefenseType type;
        public InventoryUISlot slot;
    }
    #endregion
}