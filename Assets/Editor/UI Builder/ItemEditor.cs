using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class ItemEditor : EditorWindow
{
    /// <summary>
    /// ScriptableObject类型：物品数据
    /// </summary>
    private ItemDataList_SO dataBase;
    /// <summary>
    /// <para>itemList：用来存储ItemDetails_SO添加的各个itemList</para>
    /// <para>既dataBase中对应的itemList</para>
    /// </summary>
    private List<ItemDetails> itemList = new List<ItemDetails>();
    /// <summary>
    /// ListView每一行的模板
    /// </summary>
    private VisualTreeAsset itemRowTemplate;
    /// <summary>
    /// 当前选中的物体的数据
    /// </summary>
    private ItemDetails activeItem;

    private Sprite defaultIcon;
    //UIToolKit对象
    /// <summary>
    /// UiToolKit中的ListView的对象
    /// </summary>
    private ListView itemListView;
    /// <summary>
    /// 右侧显示每个物品信息的ScrollView
    /// </summary>
    private ScrollView itemDetailsSection;
    /// <summary>
    /// 右侧窗口显示的Item图片
    /// </summary>
    private VisualElement iconPreview;
    
    //各个组件的对象
    private IntegerField selectedItemID;
    private TextField selectedItemName;
    private EnumField selectedItemType;
    private ObjectField selectedItemIcon;
    private ObjectField selectedItemSprite;
    private TextField selectedItemDescription;
    private IntegerField selectedItemUseRadius;
    private Toggle selectedItemCanPickUp;
    private Toggle selectedItemCanDropped;
    private Toggle selectedItemCanCarried;
    private IntegerField selectedItemPrice;
    private Slider selectedItemSellPresentage;
    
    [MenuItem("My Tools/ItemEditor")]
    public static void ShowExample()
    {
        //获取要显示的windos
        ItemEditor wnd = GetWindow<ItemEditor>();
        //生成窗口同时设置窗口名
        wnd.titleContent = new GUIContent("ItemEditor");
    }
    public void CreateGUI()
    {
  
        //每个编辑器窗口都包含一个根 即VisualElement对象
        //获取根节点
        VisualElement root = rootVisualElement;
        //Import UXML
        //加载xml文件
        var visualTree = 
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemEditor.uxml");
        //生成节点
        VisualElement labelFromUXML = visualTree.Instantiate();
        //添加到根节点下 完成显示
        root.Add(labelFromUXML);
        

        //获取ListView每一行的模板 使用绝对路径
        itemRowTemplate =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemRowTemplate.uxml");
        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M studio/Art/Items/Icons/icon_Game.png");
        //初始化变量
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        itemDetailsSection = root.Q<ScrollView>("ItemDetails");
        iconPreview = root.Q<VisualElement>("Icon");
        
        //获得按钮
        root.Q<Button>("AddItemBtn").clicked+=OnAddItemClicked;
        root.Q<Button>("DeleteItemBtn").clicked += OnDeleteItemClicked;
        //加载数据
        LoadDataBase();
        //生成列表
        GenerateListView();
   
    }

    private void OnDeleteItemClicked()
    {
        itemList.Remove(activeItem);
        itemListView.Rebuild();
        itemDetailsSection.visible = false;
    }

    private void OnAddItemClicked()
    {
        ItemDetails newItem = new ItemDetails();
        newItem.itemID = Settings.OrginItemID + itemList.Count;
        newItem.itemIcon = defaultIcon;
        itemList.Add(newItem);
        itemListView.Rebuild();
        
    }

    /// <summary>
    /// 加载Item数据的方法(ScriptableObjects)
    /// </summary>
    private void LoadDataBase()
    {
        //返回所有这个类型文件的guid
        var dateArray=AssetDatabase.FindAssets("ItemDataList_SO");
        
        // Debug.Log("dateArray Length"+dateArray.Length);
        
        //确定我们加载到了guid
        if (dateArray.Length > 1)
        {
            //柑橘Guid获取我们需要文件的路径
            //因为只有一个文件存储我们的item所以只需要加载第一个就可以
            var path = AssetDatabase.GUIDToAssetPath(dateArray[0]);
            //从路径加载我们的dataBase
            dataBase = AssetDatabase.LoadAssetAtPath<ItemDataList_SO>(path);
        }
        
        itemList = dataBase.itemDetailsList;
        //必须对数据进行标注 否则无法保存
        EditorUtility.SetDirty(dataBase);
        
        // Debug.Log(itemList[0].itemID);
    }

    /// <summary>
    /// UIToolKit中左边物品列表（ListView类型）的显示与更新
    /// </summary>
    private void GenerateListView()
    {
        //获取每一行的模板
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();
        //对列表中的每一行进行的操作
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if (i < itemList.Count)
            {
                //设置图片
                if(itemList[i].itemIcon!=null)
                    e.Q<VisualElement>("Icon").style.backgroundImage=itemList[i].itemIcon.texture;
                //设置名字
                e.Q<Label>("Name").text = itemList[i] == null ? "no item name" : itemList[i].itemName;
            }
        };

        //OPTIMIZE:添加到常量中
        //ListView中每一行的高度
        itemListView.fixedItemHeight = 60;
        //设置ListView的数据
        itemListView.itemsSource = itemList;
        //每一行的模板
        itemListView.makeItem= makeItem;
        //对每一行的操作
        itemListView.bindItem = bindItem;

        
        //监听
        itemListView.onSelectionChange += OnListSelectionChange;
        itemDetailsSection.visible = false;
        

    }

    private void OnListSelectionChange(IEnumerable<object> selectedItem)
    {
        activeItem = (ItemDetails)selectedItem.First();
        GetItemDetails();
        itemDetailsSection.visible = true;
    }

    //获取在ListView中选中item的详细数据
    //并将这些数据显示到右侧面板
    private void GetItemDetails()
    {
        //初始化
        selectedItemID = itemDetailsSection.Q<IntegerField>("ItemID");
        selectedItemName = itemDetailsSection.Q<TextField>("ItemName");
        selectedItemType = itemDetailsSection.Q<EnumField>("ItemType");
        selectedItemIcon = itemDetailsSection.Q<ObjectField>("ItemIcon");
        selectedItemSprite = itemDetailsSection.Q<ObjectField>("ItemSprite");
        selectedItemDescription = itemDetailsSection.Q<TextField>("Description");
        selectedItemUseRadius = itemDetailsSection.Q<IntegerField>("UseRadius");
        selectedItemCanPickUp = itemDetailsSection.Q<Toggle>("CanPickUp");
        selectedItemCanDropped = itemDetailsSection.Q<Toggle>("CanDropped");
        selectedItemCanCarried = itemDetailsSection.Q<Toggle>("CanCarried");
        selectedItemPrice = itemDetailsSection.Q<IntegerField>("Price");
        selectedItemSellPresentage = itemDetailsSection.Q<Slider>("SellPercentage");
        
        
        //这样才可以保存数据 包括可以进行撤销等操作
        itemDetailsSection.MarkDirtyRepaint();

        
        
        //itemID的设置
        
        selectedItemID.value = activeItem.itemID;
        selectedItemID.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemID = evt.newValue;
            
        });

        
        //Item名字的显示和设置
        selectedItemName.value = activeItem.itemName;
        selectedItemName.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemName = evt.newValue;
            itemListView.Rebuild();
        });
        //Item的Type的设置
        //enmu类型需要初始化
        selectedItemType.Init(activeItem.itemType);
        selectedItemType.value = activeItem.itemType;
        selectedItemType.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemType = (ItemType)evt.newValue;
        });
        //设置右侧的图片|ObjectField的图片|当前选中ListItem的图片的一致性
        iconPreview.style.backgroundImage =
            activeItem.itemIcon == null ? defaultIcon.texture : activeItem.itemIcon.texture;
        selectedItemIcon.value = activeItem.itemIcon;
        selectedItemIcon.RegisterValueChangedCallback(evt =>
        {
            Sprite newIcon = (Sprite) evt.newValue;
            
            
                //更新DataBase中的数据数据
                activeItem.itemIcon = newIcon;
                //改变右侧的图片
                iconPreview.style.backgroundImage = newIcon== null ? defaultIcon.texture : newIcon.texture;
                //改变ListView的图片
                itemListView.Rebuild();
            
        });
        //Item WorldSprite设置
        selectedItemSprite.value = activeItem.itemOnWorldSprite;
        selectedItemSprite.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemOnWorldSprite = (Sprite)evt.newValue;
        });
        //Item描述的设置
        selectedItemDescription.value = activeItem.itemDescription;
        selectedItemDescription.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemDescription = evt.newValue;
        });

        //Item使用范围的设置
        selectedItemUseRadius.value = activeItem.itemUseRadius;
        selectedItemUseRadius.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemUseRadius = evt.newValue;
        });
        //Item是否可以捡起的设置
        selectedItemCanPickUp.value = activeItem.canPickedUp;
        selectedItemCanPickUp.RegisterValueChangedCallback(evt =>
        {
            activeItem.canPickedUp = evt.newValue;
        });
        //Item是否可以扔掉的设置
        selectedItemCanDropped.value = activeItem.canDropped;
        selectedItemCanDropped.RegisterValueChangedCallback(evt =>
        {
            activeItem.canDropped = evt.newValue;
        });
        //Item是否可以拿起的设置
        selectedItemCanCarried.value = activeItem.canCarried;
        selectedItemCanCarried.RegisterValueChangedCallback(evt =>
        {
            activeItem.canCarried = evt.newValue;
        });
        //Item价格的设置
        selectedItemPrice.value = activeItem.itemPrice;
        selectedItemPrice.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemPrice = evt.newValue;
        });
        //Item出售折扣的设置
        selectedItemSellPresentage.value = activeItem.sellPercentage;
        selectedItemSellPresentage.RegisterValueChangedCallback(evt =>
        {
            activeItem.sellPercentage = evt.newValue;
        });
        
    }
}   