package com.example.accountgrpc.service;

import com.example.accountgrpc.account.AccountServiceGrpc.AccountServiceImplBase;
import com.example.accountgrpc.account.GetAccountRequest;
import com.example.accountgrpc.account.GetAccountResponse;
import io.grpc.stub.StreamObserver;
import org.springframework.grpc.server.service.GrpcService;

@GrpcService
public class MyAccountService extends AccountServiceImplBase {
    @Override
    public void getAccount(GetAccountRequest request, StreamObserver<GetAccountResponse> responseObserver) {
        responseObserver.onNext(GetAccountResponse.newBuilder()
                .setId(request.getId())
                .setName("John Doe")
                .setEmail("john.doe@example.com")
                .setAddress("123 Main St, Anytown, USA")
                .build());
        responseObserver.onCompleted();
    }
}
