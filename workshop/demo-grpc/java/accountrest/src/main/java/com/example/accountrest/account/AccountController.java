package com.example.accountrest.account;


import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class AccountController {

    @GetMapping("/v1/accounts/{id}")
    public AccountResponse getAccount(@PathVariable int id) {
        AccountResponse response = new AccountResponse();
        response.setId(id);
        response.setName("John Doe");
        response.setEmail("john.doe@example.com");
        response.setAddress("123 Main St, Anytown, USA");
        return response;
    }

}
