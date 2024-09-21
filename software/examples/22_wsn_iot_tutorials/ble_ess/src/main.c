
#include <zephyr/kernel.h>
#include <zephyr/logging/log.h>

#include <zephyr/bluetooth/bluetooth.h>
#include <zephyr/bluetooth/conn.h>
#include <zephyr/bluetooth/gatt.h>
#include <zephyr/bluetooth/uuid.h>
#include <zephyr/bluetooth/services/bas.h>

LOG_MODULE_REGISTER(ble_bas);

volatile bool ble_ready=false;
uint8_t battery_level=100;
int16_t temperature = 2412;

static const struct bt_data ad[] = 
{
	BT_DATA_BYTES(BT_DATA_FLAGS, (BT_LE_AD_GENERAL | BT_LE_AD_NO_BREDR)),
	BT_DATA_BYTES(BT_DATA_UUID16_ALL, BT_UUID_16_ENCODE(BT_UUID_BAS_VAL), BT_UUID_16_ENCODE(BT_UUID_ESS_VAL))
};

ssize_t my_read_temperature_function(struct bt_conn *conn,
					const struct bt_gatt_attr *attr, void *buf,
					uint16_t len, uint16_t offset);

BT_GATT_SERVICE_DEFINE(ess_srv,
	BT_GATT_PRIMARY_SERVICE(BT_UUID_ESS),
	BT_GATT_CHARACTERISTIC(BT_UUID_TEMPERATURE, BT_GATT_CHRC_READ, GT_GATT_PERM_READ, my_read_temperature_function, NULL, NULL),
	BT_GATT_CHARACTERISTIC(BT_UUID_PRESSURE, BT_GATT_CHRC_READ, GT_GATT_PERM_READ, NULL, NULL, NULL),
	BT_GATT_CHARACTERISTIC(BT_UUID_HUMIDITY, BT_GATT_CHRC_READ, GT_GATT_PERM_READ, NULL, NULL, NULL),
);

ssize_t my_read_temperature_function(struct bt_conn *conn,
					const struct bt_gatt_attr *attr, void *buf,
					uint16_t len, uint16_t offset)
{
	return bt_gatt_attr_read(conn, attr, buf, len, offset, &temperature, sizeof(temperature));
}

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
		k_msleep(2000);

		if (battery_level < 25)
		{
			battery_level = 100;
		}
		else
		{
			battery_level--;
		}

		bt_bas_set_battery_level(battery_level);

		printk("Hello world! %s\n", CONFIG_BOARD);
	}
	//printk("Hello world! %s\n", CONFIG_BOARD);

	return 0;
}
