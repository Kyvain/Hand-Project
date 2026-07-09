#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"
#include "MPU6050.h"
#include "Wire.h"
#include <driver/ledc.h>



#define OUTPUT_READABLE_QUATERNION
#define INTERRUPT_PIN 2
#define LED_PIN 12

bool blinkState = false;

// Переменные управления/состояния MPU
bool dmpReady = false;   // Устанавливается в true, если инициализация DMP прошла успешно
uint8_t mpuIntStatus;    // Содержит актуальный статус прерывания от MPU
uint8_t devStatus;       // Возвращает статус после каждой операции устройства (0 = успех, !0 = ошибка)
uint16_t packetSize;     // Ожидаемый размер пакета DMP (по умолчанию 42 байта)
uint16_t fifoCount;      // Количество всех байтов, текущих в FIFO
uint8_t fifoBuffer[64];  // Буфер хранения FIFO

// Переменные ориентации/движения
Quaternion q;         // [w, x, y, z] контейнер кватернионов
VectorInt16 aa;       // [x, y, z] измерения акселерометра
VectorInt16 aaReal;   // [x, y, z] измерения акселерометра без гравитации
VectorInt16 aaWorld;  // [x, y, z] измерения акселерометра в мировой системе координат
VectorFloat gravity;  // [x, y, z] вектор гравитации
float euler[3];       // [psi, theta, phi] контейнер углов Эйлера
float ypr[3];         // [yaw, pitch, roll] контейнер углов поворота и вектор гравитации
uint8_t teapotPacket[14] = { '$', 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0x00, 0x00, '\r', '\n' };

// ***********ПРЕРЫВАНИЕ*********
volatile bool mpuInterrupt = false;  // Показывает, было ли активировано прерывание MPU
void dmpDataReady() {
  mpuInterrupt = true;
}

float RateRoll, RatePitch, RateYaw;
float AccX, AccY, AccZ;
float AngleRoll, AnglePitch;
float LoopTimer;

// Пины мультиплексора
const int S0_PIN = 26;  // Пин выбора канала S0
const int S1_PIN = 25;  // Пин выбора канала S1
const int S2_PIN = 33;  // Пин выбора канала S2
const int S3_PIN = 32;  // Пин выбора канала S3
const int SIG_PIN = 27; // Выходной сигнал мультиплексора
const int EN_PIN = 25;  // Пин включения/выключения мультиплексора

const int MOTOR_PINS[] = {14, 15, 2, 4, 5, 18, 19, 12, 13, 23};
MPU6050 mpu;

const int hallThumbMin = 1750;
const int hallThumbMax = 1850;

// Функция для выбора канала мультиплексора
void selectMultiplexerChannel(int channel) {
  digitalWrite(S0_PIN, bitRead(channel, 0)); // Установка состояния бита 0 канала
  digitalWrite(S1_PIN, bitRead(channel, 1)); // Установка состояния бита 1 канала
  digitalWrite(S2_PIN, bitRead(channel, 2)); // Установка состояния бита 2 канала
  digitalWrite(S3_PIN, bitRead(channel, 3)); // Установка состояния бита 3 канала
}

// Функция для чтения значения с выбранного канала мультиплексора
int readMultiplexerChannel(int channel) {
  selectMultiplexerChannel(channel); // Выбор канала мультиплексора
  delay(5);                          // Задержка для стабилизации readings
  return analogRead(SIG_PIN);         // Чтение значения с выхода мультиплексора
}

void setup() 
{

  // for (int i = 0; i < 10; i++) {
  //   pinMode(MOTOR_PINS[i], OUTPUT); // Настройка пинов как выходные
  //   analogWriteFrequency(MOTOR_PINS[i], 500); // Установка частоты 500Hz
  //   analogWriteResolution(MOTOR_PINS[i], 8); // Установка разрешения 8 бит
  //   analogWrite(MOTOR_PINS[i], 0); // Инициализация с выключенным мотором
  // }


  Wire.begin(21, 22); // Инициализация I2C
  // Wire.setClock(100000);
  Serial.begin(9600); // Начало последовательной связи со скоростью 9600 бод
  mpu.initialize();   // Инициализация MPU6050


  pinMode(INTERRUPT_PIN, INPUT); // Настройка пина прерывания как входного

  // Настройка мультиплексора
  pinMode(S0_PIN, OUTPUT);
  pinMode(S1_PIN, OUTPUT);
  pinMode(S2_PIN, OUTPUT);
  pinMode(S3_PIN, OUTPUT);
  pinMode(EN_PIN, OUTPUT);
  digitalWrite(EN_PIN, LOW); // Включение мультиплексора (LOW = включено)

  //****DMP****
  devStatus = mpu.dmpInitialize(); // Инициализация DMP
  mpu.setXAccelOffset(-3127);
  mpu.setYAccelOffset(-2155);
  mpu.setZAccelOffset(1177);
  mpu.setXGyroOffset(256);
  mpu.setYGyroOffset(-83);
  mpu.setZGyroOffset(8);

  if (devStatus == 0) {
    mpu.CalibrateAccel(6); // Калибровка акселерометра
    mpu.CalibrateGyro(6);  // Калибровка гироскопа
    mpu.PrintActiveOffsets(); // Печать активных смещений
    Serial.println(F("Включение DMP..."));
    mpu.setDMPEnabled(true); // Включение DMP
    attachInterrupt(digitalPinToInterrupt(INTERRUPT_PIN), dmpDataReady, RISING); // Привязка прерывания
    mpuIntStatus = mpu.getIntStatus();
    Serial.println(F("DMP готов! Ожидание первого прерывания..."));
    dmpReady = true;

    packetSize = mpu.dmpGetFIFOPacketSize(); // Получение размера пакета DMP
  } else {
    Serial.print(F("Инициализация DMP не удалась (код "));
    Serial.print(devStatus);
    Serial.println(F(")"));
  }


  // if (Serial.available() > 0) {
  //   String command = Serial.readStringUntil('\n'); // Чтение команды до символа новой строки
  //   command.trim();                                // Удаление пробелов и спецсимволов
    
  //   // Пример команды: "V3,200" (активировать мотор 3 с силой 200/255)
  //   if (command.startsWith("V")) {
  //     int commaIndex = command.indexOf(',');
  //     if (commaIndex != -1) {
  //       int motorIndex = command.substring(1, commaIndex).toInt() - 1; // Номер мотора (0-4)
  //       int power = command.substring(commaIndex + 1).toInt();         // Сила вибрации (0-255)
        
  //       if (motorIndex >= 0 && motorIndex < 5 && power >= 0 && power <= 255) {
  //         ledcWrite(motorIndex, power); // Установка мощности вибрации через ШИМ
  //       }
  //     }
  //   }
  // }
  
  // Настройка остальных пинов
  pinMode(LED_PIN, OUTPUT);
    if (!mpu.testConnection()) 
  {
      Serial.println("MPU6050 not connected!");
      while (1);
  }
}


void loop() {
  // Чтение данных с мультиплексора (потенциометры)
  int PinkyFinger_raw = readMultiplexerChannel(7);   // Channel 7 - потенциометр
  int RingFinger_raw = readMultiplexerChannel(9);   // Channel 9 - потенциометр
  int MiddleFinger_raw = readMultiplexerChannel(11);   // Channel 11 - потенциометр 
  int IndexFinger_raw = readMultiplexerChannel(13); // Channel 13 - потенциометр
  int ThumbFinger_raw = readMultiplexerChannel(15);    // Channel 15 - потенциометр



  // Чтение данных с мультиплексора (датчики Холла)
  int hall_PinkyFinger_raw = readMultiplexerChannel(8);  // Channel 8 - мизинец
  int hall_RingFinger_raw = readMultiplexerChannel(10); // Channel 10 - безымянный палец
  int hall_MiddleFinger_raw = readMultiplexerChannel(12);  // Channel 12 - средний палец
  int hall_IndexFinger_raw = readMultiplexerChannel(14); // Channel 14 - указательный палец
  int hall_ThumbFinger_raw = readMultiplexerChannel(6);    // Channel 6 - большой палец





  if (Serial.available() > 0) {
    String command = Serial.readStringUntil('\n');
    command.trim();
    if (command.startsWith("V")) { // Обработка команды вибрации
      int commaIndex = command.indexOf(',');
      if (commaIndex != -1) {
        int motorIndex = command.substring(1, commaIndex).toInt();
        int power = command.substring(commaIndex + 1).toInt();

        if (motorIndex >= 0 && motorIndex < 10 && power >= 0 && power <= 255) {
          ledcWrite(motorIndex, power); // Управление вибрацией
        }
      }
    }
  }

  // Преобразование readings потенциометров в диапазон [0, 50]
  int PinkyFinger = map(constrain(PinkyFinger_raw, 0, 4095), 0, 4095, 0, 50);
  int RingFinger = map(constrain(RingFinger_raw, 0, 4095), 0, 4095, 0, 50);
  int MiddleFinger = map(constrain(MiddleFinger_raw, 0, 4095), 0, 4095, 0, 50);
  int IndexFinger = map(constrain(IndexFinger_raw, 0, 4095), 0, 4095, 0, 50);
  int ThumbFinger = map(constrain(ThumbFinger_raw, 0, 4095), 0, 4095, 0, 50);

  // Преобразование readings датчиков Холла относительно базового значения (1800)
  // int hall_PinkyFinger = constrain(hall_PinkyFinger_raw, 0, 3600);
  // int hall_RingFinger = constrain(hall_RingFinger_raw, 0, 3600);
  // int hall_MiddleFinger = constrain(hall_MiddleFinger_raw, 0, 3600);
  // int hall_IndexFinger = constrain(hall_IndexFinger_raw, 0, 3600);
  // int hall_ThumbFinger = constrain(hall_ThumbFinger_raw, 0, 3600); // Ограничение значений [0, 3600]




  // Вычисление отклонений от базового значения (1800)
  float hall_PinkyFinger = map(constrain(hall_PinkyFinger_raw, 0, 3600), 1900, 1945, 0, 50); 
  float hall_RingFinger = map(constrain(hall_RingFinger_raw, 0, 3600), 1915, 1960, 0, 50);
  float hall_MiddleFinger = map(constrain(hall_MiddleFinger_raw, 0, 3600), 1815, 1845, 0, 50);
  float hall_IndexFinger = map(constrain(hall_IndexFinger_raw, 0, 3600), 1830, 1900, 0, 50);
  float hall_ThumbFinger = map(constrain(hall_ThumbFinger_raw, 0, 3600), 1795, 1925, 0, 50);



  // Serial.print("Идут кватеры ->");
  // // Чтение данных с MPU6050
  if (mpu.dmpGetCurrentFIFOPacket(fifoBuffer)) {
    mpu.dmpGetQuaternion(&q, fifoBuffer); // Получение кватерниона
    Serial.print(q.w);
    Serial.print(",");
    Serial.print(q.x);
    Serial.print(",");
    Serial.print(q.y);
    Serial.print(",");
    Serial.print(q.z);
    Serial.print(",");
  }
  // Serial.print("Идут потенциометры ->");
  // Отправка readings потенциометров по UART
  Serial.print(PinkyFinger);
  Serial.print(",");
  Serial.print(RingFinger);
  Serial.print(",");
  Serial.print(MiddleFinger);
  Serial.print(",");
  Serial.print(IndexFinger);
  Serial.print(",");
  Serial.print(ThumbFinger);
  Serial.print(",");
  // Serial.print("Идут холлы ->");
  // // Отправка readings датчиков Холла по UART
  Serial.print(hall_PinkyFinger);
  Serial.print(",");
  Serial.print(hall_RingFinger);
  Serial.print(",");
  Serial.print(hall_MiddleFinger);
  Serial.print(",");
  Serial.print(hall_IndexFinger);
  Serial.print(",");
  Serial.println(hall_ThumbFinger);
  delay(100); // Задержка для стабильности readings
}