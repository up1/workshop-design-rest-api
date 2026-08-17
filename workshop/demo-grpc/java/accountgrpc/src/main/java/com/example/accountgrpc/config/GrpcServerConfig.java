package com.example.accountgrpc.config;

import io.grpc.netty.NettyServerBuilder;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.grpc.server.ServerBuilderCustomizer;

import java.util.concurrent.Executors;

@Configuration
public class GrpcServerConfig {

	// default executor is a small common pool; size it for expected concurrent VUs
	@Bean
	public ServerBuilderCustomizer<NettyServerBuilder> grpcServerCustomizer() {
		return builder -> builder
				.executor(Executors.newFixedThreadPool(200))
				.maxConcurrentCallsPerConnection(200);
	}

}
