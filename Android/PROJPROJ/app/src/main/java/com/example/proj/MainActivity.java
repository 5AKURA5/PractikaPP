package com.example.proj;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import androidx.appcompat.app.AppCompatActivity;

public class MainActivity extends AppCompatActivity {

    private EditText loginInput;
    private EditText passwordInput;
    private Button loginButton;
    private Button registerButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        loginInput = findViewById(R.id.loginEditText);
        passwordInput = findViewById(R.id.passwordEditText);
        loginButton = findViewById(R.id.loginButton);
        registerButton = findViewById(R.id.registerButton);

        loginButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String login = loginInput.getText().toString();
                String password = passwordInput.getText().toString();

                if (login.equals("admin") && password.equals("admin123")) {
                    // Переход на форму администратора
                    Intent intent = new Intent(MainActivity.this, client_activity.class);
                    startActivity(intent);
                } else if (login.equals("client") && password.equals("client123")) {
                    // Переход на форму клиента
                    Intent intent = new Intent(MainActivity.this, admin_activity.class);
                    startActivity(intent);
                } else if (login.equals("employee") && password.equals("employee123")) {
                    // Переход на форму сотрудника
                    Intent intent = new Intent(MainActivity.this, worker_activity.class);
                    startActivity(intent);
                } else {
                    // Ошибка авторизации
                    // Здесь можно добавить обработку ошибки
                }
            }
        });

        registerButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Переход на форму регистрации
                Intent intent = new Intent(MainActivity.this, register_activity.class);
                startActivity(intent);
            }
        });
    }
}