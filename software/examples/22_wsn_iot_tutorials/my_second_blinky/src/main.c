
#include <stdio.h>
#include <zephyr/kernel.h>
#include <zephyr/drivers/gpio.h>
#include <zephyr/sys/printk.h>
#include <hal/nrf_gpio.h>

/*
#define SLEEP_TIME_MS   1000
//#define GPIO_PIN		NRF_GPIO_PIN_MAP(0,27) //for externally connected LED
#define GPIO_PIN		NRF_GPIO_PIN_MAP(0,14) //for on board LED


int main(void)
{
	nrf_gpio_cfg_output(GPIO_PIN); //set pin as output
	while (1) {
		//sleep and print
		// printk("Hello world!");
		// k_msleep(SLEEP_TIME_MS);

		nrf_gpio_pin_set(GPIO_PIN);			//set pin to HIGH (LED off)
		k_msleep(SLEEP_TIME_MS);
		nrf_gpio_pin_clear(GPIO_PIN);		//set pin to LOW (LED on)
		k_msleep(SLEEP_TIME_MS);
	}
	return 0;
}*/


#define SLEEP_TIME_MS   1000
#define P0_BASE_ADDRESS	0x50000000

// Offset for both registers
#define GPIO_DIR_OFFSET 0x514
#define GPIO_OUT_OFFSET 0x504

//bit position
#define PIN_13_BIT_POSITION 13

int main(void)
{
	//set port 0 pin 14 as output
	volatile uint32_t *p0_dir_reg = (volatile uint32_t *)(P0_BASE_ADDRESS + GPIO_DIR_OFFSET);
	*p0_dir_reg |= (1 << PIN_13_BIT_POSITION);

	volatile uint32_t *p0_out_reg = (volatile uint32_t *)(P0_BASE_ADDRESS + GPIO_OUT_OFFSET);

	while (1)
	{
		*p0_out_reg |= (1 << PIN_13_BIT_POSITION); //on
		k_msleep(SLEEP_TIME_MS);
		*p0_out_reg &= ~(1 << PIN_13_BIT_POSITION); //off
		k_msleep(SLEEP_TIME_MS);
		printk("Low Level LED!\n");
	}
}