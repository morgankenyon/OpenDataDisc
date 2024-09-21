#include <zephyr/kernel.h>
#include <zephyr/logging/log.h>

#include <zephyr/bluetooth/bluetooth.h>
#include <zephyr/bluetooth/conn.h>
#include <zephyr/bluetooth/gatt.h>
#include <zephyr/bluetooth/uuid.h>

LOG_MODULE_REGISTER(ble_bas);

// UUID of the custom service
#define BT_UUIT_MY_CUSTOM_SERV_VAL BT_UUID_128_ENCODE(0x900e9509, 0xa0b2, 0x4d89, 0x9bb6, 0xb5e011e758b0)
#define BT_UUID_MY_CUSTOM_SERVICE BT_UUID_DECLARE_128(BT_UUIT_MY_CUSTOM_SERV_VAL)

// UUID of the custom temperature characteristic
#define BT_UUIT_MY_TEMPERATURE_CHRC_VAL BT_UUID_128_ENCODE(0x6ef4cd45, 0x7223, 0x43b2, 0xb5c9, 0xd13410b494f5)
#define BT_UUID_MY_TEMPERATURE_CHRC BT_UUID_DECLARE_128(BT_UUIT_MY_TEMPERATURE_CHRC_VAL)

#define VND_MAX_LEN 20

volatile bool ble_ready=false;

int32_t temperature = 241234;

static const struct bt_data ad[] = 
{
	BT_DATA_BYTES(BT_DATA_FLAGS, (BT_LE_AD_GENERAL | BT_LE_AD_NO_BREDR)),
	BT_DATA_BYTES(BT_DATA_UUID128_ALL, BT_UUIT_MY_CUSTOM_SERV_VAL)
};

BT_GATT_SERVICE_DEFINE(custom_srv,
	BT_GATT_PRIMARY_SERVICE(BT_UUID_MY_CUSTOM_SERVICE),
	BT_GATT_CHARACTERISTIC(BT_UUID_MY_TEMPERATURE_CHRC, BT_GATT_CHRC_NOTIFY, BT_GATT_PERM_NONE, NULL, NULL, NULL),
	BT_GATT_CCC(NULL, BT_GATT_PERM_READ | BT_GATT_PERM_WRITE),
);

void bt_ready(int err)
{
	if (err)
	{
		LOG_ERR("bt enable returns %d", err);
	}

	LOG_INF("bt_ready!\n");
	ble_ready = true;
}

int init_ble(void)
{
	LOG_INF("Init BLE");
	int err;

	LOG_INF("AfterInit BLE");
	//bt_conn_cb_register(&conn_callbacks);

	err = bt_enable(bt_ready);
	
	LOG_INF("After bt_enable");
	if (err)
	{
		LOG_INF("Also an error");
		LOG_ERR("bt_enable failed (err %d)", err);
		return err;
	}

	
	LOG_INF("returning");

	return 0;
}

static int notify_adc(void)
{
	printk("notify atc2 ");
	static uint8_t level[20] = { 'A', '0', '.', '2', '8', ',', '0', '.', '1', '4', ',', '1', '.', '8', '2', };

	int rc;
	rc = bt_gatt_notify(NULL, &custom_srv.attrs[2], &level, sizeof(level));

	return rc == -ENOTCONN ? 0 : rc;
}

int main(void)
{
	printk("Hello world! %s\n", CONFIG_BOARD);
	init_ble();

	while (!ble_ready)
	{
		LOG_INF("BLE stack not ready yet");
		k_msleep(100);
	}
	LOG_INF("BLE stack ready");

	int err;
	err = bt_le_adv_start(BT_LE_ADV_CONN_NAME, ad, ARRAY_SIZE(ad), NULL, 0);
	if (err)
	{
		printk("Advertising failed to start (err %d)\n", err);
		return 1;
	}

	while (true)
	{
		//k_msleep(2000);

		//printk("Hello world! %s\n", CONFIG_BOARD);
		notify_adc();
	}
	//printk("Hello world! %s\n", CONFIG_BOARD);

	return 0;
}
